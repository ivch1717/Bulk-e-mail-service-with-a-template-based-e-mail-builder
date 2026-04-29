using Npgsql;

namespace MailSender;

public sealed class PostgresOutboxRepository : IOutboxRepository
{
    private readonly string _connectionString;

    public PostgresOutboxRepository(PostgresOptions options)
    {
        _connectionString = options.ConnectionString;
    }

    public async Task InitializeAsync(CancellationToken ct)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(ct);

        const string sql = """
            create table if not exists mail_sender_state (
                outbox_id uuid primary key,
                published_at timestamp with time zone null,
                publish_lease_until timestamp with time zone null,
                publish_attempts integer not null default 0,
                delivery_lease_until timestamp with time zone null,
                delivery_attempts integer not null default 0,
                next_retry_at timestamp with time zone null,
                last_attempt_at timestamp with time zone null,
                sent_at timestamp with time zone null,
                dead_lettered_at timestamp with time zone null,
                last_error text null,
                created_at timestamp with time zone not null default now(),
                updated_at timestamp with time zone not null default now()
            );

            create index if not exists ix_mail_sender_state_publish
                on mail_sender_state (published_at, publish_lease_until);

            create index if not exists ix_mail_sender_state_delivery
                on mail_sender_state (delivery_lease_until, next_retry_at, dead_lettered_at);
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync(ct);
    }

    public async Task<IReadOnlyList<OutboxEmail>> AcquirePublishBatchAsync(
        int batchSize,
        TimeSpan leaseDuration,
        CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var leaseUntil = now.Add(leaseDuration);
        var normalizedBatchSize = Math.Max(1, batchSize);
        var result = new List<OutboxEmail>(normalizedBatchSize);

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(ct);

        const string sql = """
            with candidates as (
                select o."Id", o."To", o."Html", o."CreatedAt"
                from "OutboxEmails" o
                left join mail_sender_state s on s.outbox_id = o."Id"
                where o."Sent" = false
                  and (
                      s.outbox_id is null
                      or (
                          s.sent_at is null
                          and s.dead_lettered_at is null
                          and s.published_at is null
                          and (s.publish_lease_until is null or s.publish_lease_until < @now)
                      )
                  )
                order by o."CreatedAt", o."Id"
                limit @batchSize
            ),
            ensured as (
                insert into mail_sender_state (outbox_id, created_at, updated_at)
                select c."Id", @now, @now
                from candidates c
                on conflict (outbox_id) do nothing
            ),
            claimed as (
                update mail_sender_state s
                set publish_lease_until = @leaseUntil,
                    publish_attempts = s.publish_attempts + 1,
                    updated_at = @now
                from candidates c
                where s.outbox_id = c."Id"
                  and s.sent_at is null
                  and s.dead_lettered_at is null
                  and s.published_at is null
                  and (s.publish_lease_until is null or s.publish_lease_until < @now)
                returning s.outbox_id
            )
            select c."Id", c."To", c."Html", c."CreatedAt"
            from candidates c
            join claimed cl on cl.outbox_id = c."Id"
            order by c."CreatedAt", c."Id"
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("now", now);
        command.Parameters.AddWithValue("leaseUntil", leaseUntil);
        command.Parameters.AddWithValue("batchSize", normalizedBatchSize);

        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            result.Add(new OutboxEmail(
                reader.GetGuid(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetDateTime(3),
                Sent: false));
        }

        return result;
    }

    public async Task MarkPublishedAsync(Guid messageId, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(ct);

        const string sql = """
            update mail_sender_state
            set published_at = @now,
                publish_lease_until = null,
                last_error = null,
                updated_at = @now
            where outbox_id = @id
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", messageId);
        command.Parameters.AddWithValue("now", now);
        await command.ExecuteNonQueryAsync(ct);
    }

    public async Task ReleasePublishLeaseAsync(Guid messageId, string? error, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(ct);

        const string sql = """
            update mail_sender_state
            set publish_lease_until = null,
                last_error = @error,
                updated_at = @now
            where outbox_id = @id
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", messageId);
        command.Parameters.AddWithValue("error", (object?)error ?? DBNull.Value);
        command.Parameters.AddWithValue("now", now);
        await command.ExecuteNonQueryAsync(ct);
    }

    public async Task<OutboxDeliveryAttempt> TryAcquireDeliveryAsync(
        Guid messageId,
        TimeSpan leaseDuration,
        CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var leaseUntil = now.Add(leaseDuration);

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(ct);

        await EnsureStateRowAsync(connection, messageId, now, ct);

        const string acquireSql = """
            update mail_sender_state s
            set delivery_lease_until = @leaseUntil,
                updated_at = @now
            from "OutboxEmails" o
            where s.outbox_id = @id
              and o."Id" = @id
              and o."Sent" = false
              and s.sent_at is null
              and s.dead_lettered_at is null
              and (s.delivery_lease_until is null or s.delivery_lease_until < @now)
            returning s.delivery_attempts
            """;

        await using (var acquire = new NpgsqlCommand(acquireSql, connection))
        {
            acquire.Parameters.AddWithValue("id", messageId);
            acquire.Parameters.AddWithValue("now", now);
            acquire.Parameters.AddWithValue("leaseUntil", leaseUntil);

            var raw = await acquire.ExecuteScalarAsync(ct);
            if (raw is not null and not DBNull)
            {
                return new OutboxDeliveryAttempt(DeliveryAcquireStatus.Ready, (int)raw);
            }
        }

        const string statusSql = """
            select
                case
                    when o."Id" is null or o."Sent" = true or s.sent_at is not null or s.dead_lettered_at is not null
                        then 1
                    else 2
                end as status,
                coalesce(s.delivery_attempts, 0) as delivery_attempts
            from (select 1) x
            left join "OutboxEmails" o on o."Id" = @id
            left join mail_sender_state s on s.outbox_id = @id
            """;

        await using var status = new NpgsqlCommand(statusSql, connection);
        status.Parameters.AddWithValue("id", messageId);

        await using var reader = await status.ExecuteReaderAsync(ct);
        if (await reader.ReadAsync(ct))
        {
            return new OutboxDeliveryAttempt(
                (DeliveryAcquireStatus)reader.GetInt32(0),
                reader.GetInt32(1));
        }

        return new OutboxDeliveryAttempt(DeliveryAcquireStatus.Completed, 0);
    }

    public async Task MarkDeliveredAsync(Guid messageId, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);

        const string markOutboxSql = """
            update "OutboxEmails"
            set "Sent" = true
            where "Id" = @id and "Sent" = false
            """;

        await using (var outbox = new NpgsqlCommand(markOutboxSql, connection, transaction))
        {
            outbox.Parameters.AddWithValue("id", messageId);
            await outbox.ExecuteNonQueryAsync(ct);
        }

        const string markStateSql = """
            update mail_sender_state
            set sent_at = @now,
                delivery_lease_until = null,
                next_retry_at = null,
                last_attempt_at = @now,
                last_error = null,
                updated_at = @now
            where outbox_id = @id
            """;

        await using (var state = new NpgsqlCommand(markStateSql, connection, transaction))
        {
            state.Parameters.AddWithValue("id", messageId);
            state.Parameters.AddWithValue("now", now);
            await state.ExecuteNonQueryAsync(ct);
        }

        await transaction.CommitAsync(ct);
    }

    public async Task<DeliveryFailureResult> MarkDeliveryFailedAsync(
        Guid messageId,
        string error,
        TimeSpan retryDelay,
        int maxDeliveryAttempts,
        CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var nextRetryAt = now.Add(retryDelay);

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(ct);

        const string sql = """
            update mail_sender_state
            set delivery_lease_until = null,
                delivery_attempts = delivery_attempts + 1,
                next_retry_at = case
                    when delivery_attempts + 1 >= @maxAttempts then null
                    else @nextRetryAt
                end,
                last_attempt_at = @now,
                dead_lettered_at = case
                    when delivery_attempts + 1 >= @maxAttempts then @now
                    else dead_lettered_at
                end,
                last_error = @error,
                updated_at = @now
            where outbox_id = @id
            returning delivery_attempts, dead_lettered_at is not null
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", messageId);
        command.Parameters.AddWithValue("error", error);
        command.Parameters.AddWithValue("now", now);
        command.Parameters.AddWithValue("nextRetryAt", nextRetryAt);
        command.Parameters.AddWithValue("maxAttempts", Math.Max(1, maxDeliveryAttempts));

        await using var reader = await command.ExecuteReaderAsync(ct);
        if (await reader.ReadAsync(ct))
        {
            return new DeliveryFailureResult(
                reader.GetInt32(0),
                reader.GetBoolean(1));
        }

        return new DeliveryFailureResult(Math.Max(1, maxDeliveryAttempts), true);
    }

    private static async Task EnsureStateRowAsync(
        NpgsqlConnection connection,
        Guid messageId,
        DateTime now,
        CancellationToken ct)
    {
        const string sql = """
            insert into mail_sender_state (outbox_id, created_at, updated_at)
            values (@id, @now, @now)
            on conflict (outbox_id) do nothing
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", messageId);
        command.Parameters.AddWithValue("now", now);
        await command.ExecuteNonQueryAsync(ct);
    }
}
