using Npgsql;

namespace MailSender;

public sealed class OutboxDeliveryLease : IAsyncDisposable
{
    private readonly NpgsqlConnection _connection;
    private readonly long _lockKey;
    private bool _disposed;

    internal OutboxDeliveryLease(Guid messageId, NpgsqlConnection connection, long lockKey, bool exists, bool isSent)
    {
        MessageId = messageId;
        _connection = connection;
        _lockKey = lockKey;
        Exists = exists;
        IsSent = isSent;
    }

    public Guid MessageId { get; }

    public bool Exists { get; }

    public bool IsSent { get; }

    internal NpgsqlConnection Connection => _connection;

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        try
        {
            await using var unlock = new NpgsqlCommand("select pg_advisory_unlock(@key)", _connection);
            unlock.Parameters.AddWithValue("key", _lockKey);
            await unlock.ExecuteScalarAsync();
        }
        catch
        {
            // ignored
        }
        finally
        {
            await _connection.DisposeAsync();
        }
    }
}
