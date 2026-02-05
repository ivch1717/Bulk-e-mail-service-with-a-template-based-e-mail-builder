namespace MailSender;

public sealed record EmailSendRequested(
    Guid MessageId,
    string To,
    string Subject,
    string HtmlBody,
    string? FromEmail,
    string? FromName
);