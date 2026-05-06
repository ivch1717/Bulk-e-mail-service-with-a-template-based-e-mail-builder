namespace MailSender;

public sealed record SmtpProfileSummary(
    Guid Id,
    string User,
    string FromEmail,
    string DisplayName
);
