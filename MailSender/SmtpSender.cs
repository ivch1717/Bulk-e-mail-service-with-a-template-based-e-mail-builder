using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace MailSender;

public sealed class SmtpSender : ISmtpSender, IAsyncDisposable
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private SmtpClient? _client;

    private readonly string _host = GetEnvVar("SMTP_HOST");
    private readonly int _port = int.Parse(GetEnvVar("SMTP_PORT", "587"));
    private readonly string _user = GetEnvVar("SMTP_USER");
    private readonly string _pass = GetEnvVar("SMTP_PASS");
    private readonly bool _startTls = bool.Parse(GetEnvVar("SMTP_USE_STARTTLS", "true"));

    private readonly string? _defaultFromEmail = Environment.GetEnvironmentVariable("SMTP_FROM_EMAIL");
    private readonly string? _defaultFromName  = Environment.GetEnvironmentVariable("SMTP_FROM_NAME");

    private static string GetEnvVar(string key, string? def = null) =>
        Environment.GetEnvironmentVariable(key) ?? def
        ?? throw new InvalidOperationException($"{key} env var is required");

    public async Task SendAsync(EmailSendRequested msg, CancellationToken ct)
    {
        var fromEmail = msg.FromEmail ?? _defaultFromEmail ?? _user;
        var fromName  = msg.FromName  ?? _defaultFromName  ?? "MailSender";

        var m = new MimeMessage();
        m.From.Add(new MailboxAddress(fromName, fromEmail));
        m.To.Add(MailboxAddress.Parse(msg.To));
        m.Subject = msg.Subject ?? "";
        m.Body = new BodyBuilder { HtmlBody = msg.HtmlBody ?? "" }.ToMessageBody();

        await _lock.WaitAsync(ct);
        try
        {
            _client ??= new SmtpClient();
            if (!_client.IsConnected)
            {
                var opt = _startTls ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;
                await _client.ConnectAsync(_host, _port, opt, ct);
                await _client.AuthenticateAsync(_user, _pass, ct);
            }
            await _client.SendAsync(m, ct);
        }
        catch
        {
            if (_client is not null)
            {
                try { await _client.DisconnectAsync(true, ct); }
                catch
                {
                    // ignored
                }

                _client.Dispose();
                _client = null;
            }
            throw;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_client is not null)
        {
            try { await _client.DisconnectAsync(true); } catch { }
            _client.Dispose();
        }
        _lock.Dispose();
    }
}
