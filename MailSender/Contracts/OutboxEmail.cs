namespace MailSender;

public sealed record OutboxEmail(
    Guid Id,
    Guid SmtpProfileId,
    string To,
    string Html,
    string Subject,
    DateTime CreatedAt,
    bool Sent
);
