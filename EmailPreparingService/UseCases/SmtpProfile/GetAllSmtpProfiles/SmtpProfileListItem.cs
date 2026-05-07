namespace UseCases.SmtpProfile.GetAllSmtpProfiles;

public record SmtpProfileListItem(Guid id, string user, string fromEmail, string displayName);