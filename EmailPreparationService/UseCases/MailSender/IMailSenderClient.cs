using UseCases.SmtpProfile.CreateSmtpProfile;

namespace UseCases.MailSender;

public interface IMailSenderClient
{
    Task<IReadOnlyList<MailSenderSmtpProfile>> GetSmtpProfilesAsync();
    Task<Guid> CreateSmtpProfileAsync(CreateSmtpProfileRequest request);
    Task DeleteSmtpProfileAsync(Guid id);
}