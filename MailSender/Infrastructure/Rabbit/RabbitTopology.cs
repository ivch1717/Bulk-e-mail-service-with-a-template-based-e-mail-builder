using RabbitMQ.Client;

namespace MailSender;

internal static class RabbitTopology
{
    public static async Task DeclareAsync(
        IChannel channel,
        RabbitOptions options,
        OutboxProcessingOptions outboxOptions,
        CancellationToken ct)
    {
        await channel.ExchangeDeclareAsync(
            options.Exchange,
            ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: ct);

        await channel.ExchangeDeclareAsync(
            options.RetryExchange,
            ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: ct);

        await channel.ExchangeDeclareAsync(
            options.DeadLetterExchange,
            ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: ct);

        await channel.QueueDeclareAsync(
            options.Queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                ["x-dead-letter-exchange"] = options.RetryExchange,
                ["x-dead-letter-routing-key"] = options.RetryRoutingKey
            },
            cancellationToken: ct);

        await channel.QueueBindAsync(
            options.Queue,
            options.Exchange,
            options.RoutingKey,
            cancellationToken: ct);

        await channel.QueueDeclareAsync(
            options.RetryQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                ["x-dead-letter-exchange"] = options.Exchange,
                ["x-dead-letter-routing-key"] = options.RoutingKey,
                ["x-message-ttl"] = Math.Max(1, outboxOptions.RetryDelaySeconds) * 1000
            },
            cancellationToken: ct);

        await channel.QueueBindAsync(
            options.RetryQueue,
            options.RetryExchange,
            options.RetryRoutingKey,
            cancellationToken: ct);

        await channel.QueueDeclareAsync(
            options.DeadLetterQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: ct);

        await channel.QueueBindAsync(
            options.DeadLetterQueue,
            options.DeadLetterExchange,
            options.DeadLetterRoutingKey,
            cancellationToken: ct);
    }
}
