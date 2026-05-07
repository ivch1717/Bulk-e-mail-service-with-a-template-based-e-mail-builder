namespace UseCases.MailSender;

public sealed record MailSenderSmtpProfile(
    Guid Id,
    string User,
    string FromEmail,
    string DisplayName
);