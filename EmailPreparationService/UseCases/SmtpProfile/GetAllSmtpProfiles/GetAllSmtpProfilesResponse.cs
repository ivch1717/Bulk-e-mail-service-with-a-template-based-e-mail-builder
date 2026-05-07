namespace UseCases.SmtpProfile.GetAllSmtpProfiles;

public record GetAllSmtpProfilesResponse(List<SmtpProfileListItem> smtpProfiles);