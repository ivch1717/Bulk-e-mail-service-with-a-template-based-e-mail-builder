namespace MailSender;

public interface ISmtpSender
{
    Task SendAsync(EmailSendRequested msg, SmtpProfile smtpProfile, CancellationToken ct);
}
