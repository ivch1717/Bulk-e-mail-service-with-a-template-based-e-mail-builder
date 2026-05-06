namespace MailSender;

public sealed record EmailSendRequested(
    Guid MessageId,
    Guid SmtpProfileId,
    string To,
    string HtmlBody,
    string Subject
);
