namespace MailSender;

public sealed record DeliveryFailureResult(
    int DeliveryAttempts,
    bool ShouldDeadLetter);
