namespace UseCases.SmtpProfile.CreateSmtpProfile;

public record CreateSmtpProfileRequest(
    string host,
    int port,
    string user,
    string password,
    string fromEmail,
    string displayName
);