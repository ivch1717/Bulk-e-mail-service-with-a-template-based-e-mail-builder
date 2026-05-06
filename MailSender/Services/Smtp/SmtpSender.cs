using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace MailSender;

public sealed class SmtpSender : ISmtpSender, IAsyncDisposable
{
    public async Task SendAsync(EmailSendRequested msg, SmtpProfile smtpProfile, CancellationToken ct)
    {
        var m = new MimeMessage();
        m.From.Add(new MailboxAddress(smtpProfile.DisplayName, smtpProfile.FromEmail));
        m.To.Add(MailboxAddress.Parse(msg.To));
        m.Subject = msg.Subject;
        m.Body = new BodyBuilder { HtmlBody = msg.HtmlBody ?? "" }.ToMessageBody();
        m.MessageId = $"<{msg.MessageId:D}@mailsender>";
        m.Headers.Add("X-Outbox-Id", msg.MessageId.ToString("D"));
        m.Headers.Add("X-Smtp-Profile-Id", smtpProfile.Id.ToString("D"));

        using var client = new SmtpClient();
        var options = smtpProfile.UseStartTls
            ? SecureSocketOptions.StartTls
            : SecureSocketOptions.Auto;

        await client.ConnectAsync(smtpProfile.Host, smtpProfile.Port, options, ct);
        if (!string.IsNullOrWhiteSpace(smtpProfile.User))
        {
            await client.AuthenticateAsync(smtpProfile.User, smtpProfile.Password, ct);
        }

        await client.SendAsync(m, ct);
        await client.DisconnectAsync(true, ct);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
