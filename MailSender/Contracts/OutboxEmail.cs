namespace MailSender;

public sealed record OutboxEmail(
    Guid Id,
    string To,
    string Html,
    DateTime CreatedAt,
    bool Sent
);
