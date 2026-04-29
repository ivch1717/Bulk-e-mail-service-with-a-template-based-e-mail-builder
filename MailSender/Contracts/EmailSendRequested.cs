namespace MailSender;

public sealed record EmailSendRequested(
    Guid MessageId,
    string To,
    string HtmlBody,
    string? Subject,
    string? FromEmail,
    string? FromName
);
