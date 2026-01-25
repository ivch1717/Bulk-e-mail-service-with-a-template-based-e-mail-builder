namespace MailSender;

public interface ISmtpSender
{
    Task SendAsync(EmailSendRequested msg, CancellationToken ct);
}