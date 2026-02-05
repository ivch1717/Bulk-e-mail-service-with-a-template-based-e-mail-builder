using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace MailSender;

public static class RabbitPublisherExample
{
    public static async Task PublishTestBatchAsync(
        RabbitOptions opt,
        string to,
        int count,
        string? subjectPrefix = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(to))
            throw new ArgumentException("Recipient address is required.", nameof(to));
        if (count <= 0) return;

        var factory = new ConnectionFactory
        {
            HostName = opt.Host,
            Port = opt.Port,
            UserName = opt.User,
            Password = opt.Pass,
        };

        await using var conn = await factory.CreateConnectionAsync(ct);
        await using var ch = await conn.CreateChannelAsync(cancellationToken: ct);

        await ch.ExchangeDeclareAsync(opt.Exchange, ExchangeType.Direct, durable: true, autoDelete: false, cancellationToken: ct);
        await ch.QueueDeclareAsync(opt.Queue, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: ct);
        await ch.QueueBindAsync(opt.Queue, opt.Exchange, opt.RoutingKey, cancellationToken: ct);

        var prefix = string.IsNullOrWhiteSpace(subjectPrefix) ? "Test" : subjectPrefix.Trim();

        for (var i = 0; i < count; i++)
        {
            var msg = new EmailSendRequested(
                Guid.NewGuid(),
                to,
                $"{prefix} #{i + 1}",
                $"<p>Test message {i + 1} at {DateTimeOffset.UtcNow:O}</p>",
                null,
                null);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(msg));
            await ch.BasicPublishAsync(opt.Exchange, opt.RoutingKey, body, ct);
        }
    }
}
