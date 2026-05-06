namespace MailSender;

public sealed record SmtpProfile(
    Guid Id,
    string Host,
    int Port,
    string User,
    string Password,
    string FromEmail,
    string DisplayName,
    bool UseStartTls
);
