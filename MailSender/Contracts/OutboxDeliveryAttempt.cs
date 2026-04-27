namespace MailSender;

public sealed record OutboxDeliveryAttempt(
    DeliveryAcquireStatus Status,
    int DeliveryAttempts);

public enum DeliveryAcquireStatus
{
    Ready = 0,
    Completed = 1,
    Busy = 2
}
