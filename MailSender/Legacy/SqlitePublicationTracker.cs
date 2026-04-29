using System.Globalization;
using Microsoft.Data.Sqlite;

namespace MailSender;

public sealed class SqlitePublicationTracker : IPublicationTracker
{
    private readonly string _path;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private volatile bool _initialized;

    public SqlitePublicationTracker()
    {
        _path = Environment.GetEnvironmentVariable("OUTBOX_TRACKER_FILE")
            ?? "data/outbox-publications.db";
    }

    public async Task<bool> TryReserveAsync(Guid messageId, DateTimeOffset now, TimeSpan staleAfter, CancellationToken ct)
    {
        await EnsureInitializedAsync(ct);

        await using var connection = OpenConnection();
        await connection.OpenAsync(ct);
        await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(ct);

        var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            select PublishedAt
            from PublishedMessages
            where MessageId = $id
            """;
        command.Parameters.AddWithValue("$id", messageId.ToString("D"));

        var existing = await command.ExecuteScalarAsync(ct);
        var publishedAt = existing as string;

        if (publishedAt is null)
        {
            await UpsertAsync(connection, transaction, messageId, now, ct);
            await transaction.CommitAsync(ct);
            return true;
        }

        var existingTimestamp = DateTimeOffset.ParseExact(
            publishedAt,
            "O",
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind);

        if (existingTimestamp > now - staleAfter)
        {
            await transaction.CommitAsync(ct);
            return false;
        }

        await UpsertAsync(connection, transaction, messageId, now, ct);
        await transaction.CommitAsync(ct);
        return true;
    }

    public Task ReleaseAsync(Guid messageId, CancellationToken ct)
        => DeleteAsync(messageId, ct);

    public Task CompleteAsync(Guid messageId, CancellationToken ct)
        => DeleteAsync(messageId, ct);

    private async Task DeleteAsync(Guid messageId, CancellationToken ct)
    {
        await EnsureInitializedAsync(ct);

        await using var connection = OpenConnection();
        await connection.OpenAsync(ct);

        var command = connection.CreateCommand();
        command.CommandText = """
            delete from PublishedMessages
            where MessageId = $id
            """;
        command.Parameters.AddWithValue("$id", messageId.ToString("D"));
        await command.ExecuteNonQueryAsync(ct);
    }

    private async Task EnsureInitializedAsync(CancellationToken ct)
    {
        if (_initialized)
        {
            return;
        }

        await _initLock.WaitAsync(ct);
        try
        {
            if (_initialized)
            {
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(_path) ?? ".");

            await using var connection = OpenConnection();
            await connection.OpenAsync(ct);

            var command = connection.CreateCommand();
            command.CommandText = """
                create table if not exists PublishedMessages (
                    MessageId text primary key,
                    PublishedAt text not null
                )
                """;
            await command.ExecuteNonQueryAsync(ct);

            _initialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }

    private async Task UpsertAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        Guid messageId,
        DateTimeOffset publishedAt,
        CancellationToken ct)
    {
        var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            insert into PublishedMessages (MessageId, PublishedAt)
            values ($id, $publishedAt)
            on conflict(MessageId) do update
            set PublishedAt = excluded.PublishedAt
            """;
        command.Parameters.AddWithValue("$id", messageId.ToString("D"));
        command.Parameters.AddWithValue("$publishedAt", publishedAt.ToString("O", CultureInfo.InvariantCulture));
        await command.ExecuteNonQueryAsync(ct);
    }

    private SqliteConnection OpenConnection()
        => new($"Data Source={_path};Mode=ReadWriteCreate;Cache=Shared");
}
