namespace MailSender;

public sealed class OutboxProcessingOptions
{
    public int BatchSize { get; init; } = 100;
    public int PollIntervalMilliseconds { get; init; } = 1000;
    public int PublishLeaseSeconds { get; init; } = 30;
    public int DeliveryLeaseSeconds { get; init; } = 120;
    public int RetryDelaySeconds { get; init; } = 30;
    public int MaxDeliveryAttempts { get; init; } = 5;
}
