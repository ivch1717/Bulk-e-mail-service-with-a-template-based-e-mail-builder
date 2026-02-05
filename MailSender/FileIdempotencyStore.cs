using System.Text;

namespace MailSender;

public sealed class FileIdempotencyStore : IIdempotencyStore
{
    private readonly string _path;
    private readonly HashSet<Guid> _processed = new();
    private readonly HashSet<Guid> _inflight  = new();
    private readonly object _lock = new();

    public FileIdempotencyStore()
    {
        _path = Environment.GetEnvironmentVariable("IDEM_FILE")
            ?? "data/processed-ids.log";
    }

    public async Task InitAsync(CancellationToken ct)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path) ?? ".");

        if (!File.Exists(_path)) return;

        var lines = await File.ReadAllLinesAsync(_path, ct);
        lock (_lock)
        {
            foreach (var line in lines)
                if (Guid.TryParse(line.Trim(), out var id))
                    _processed.Add(id);
        }
    }

    public bool TryBegin(Guid messageId)
    {
        lock (_lock)
        {
            if (_processed.Contains(messageId)) return false;
            if (_inflight.Contains(messageId))  return false;

            _inflight.Add(messageId);
            return true;
        }
    }

    public async Task MarkSuccessAsync(Guid messageId, CancellationToken ct)
    {
        lock (_lock)
        {
            _inflight.Remove(messageId);
            _processed.Add(messageId);
        }

        var bytes = Encoding.UTF8.GetBytes(messageId.ToString("D") + "\n");
        await using var fs = new FileStream(_path, FileMode.Append, FileAccess.Write, FileShare.Read);
        await fs.WriteAsync(bytes, ct);
        await fs.FlushAsync(ct);
    }

    public void MarkFailed(Guid messageId)
    {
        lock (_lock)
            _inflight.Remove(messageId);
    }
}
