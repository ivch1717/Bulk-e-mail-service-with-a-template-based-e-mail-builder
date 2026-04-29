namespace MailSender;

public sealed class RabbitOptions
{
    public string Host { get; init; } = "rabbitmq";
    public int Port { get; init; } = 5672;
    public string User { get; init; } = "guest";
    public string Pass { get; init; } = "guest";

    public string Exchange { get; init; } = "mail.send";
    public string Queue { get; init; } = "mail.send.q";
    public string RoutingKey { get; init; } = "send";
    public string RetryExchange { get; init; } = "mail.send.retry";
    public string RetryQueue { get; init; } = "mail.send.retry.q";
    public string RetryRoutingKey { get; init; } = "send.retry";
    public string DeadLetterExchange { get; init; } = "mail.send.dlx";
    public string DeadLetterQueue { get; init; } = "mail.send.dlq";
    public string DeadLetterRoutingKey { get; init; } = "send.dlq";
    public ushort Prefetch { get; init; } = 10;

    public int Consumers { get; init; } = 5;
}
