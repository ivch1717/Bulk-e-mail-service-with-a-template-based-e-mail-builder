namespace MailSender;

public sealed record CreateSmtpProfileRequest(
    string Host,
    int Port,
    string User,
    string Password,
    string FromEmail,
    string DisplayName,
    bool UseStartTls = true
);
