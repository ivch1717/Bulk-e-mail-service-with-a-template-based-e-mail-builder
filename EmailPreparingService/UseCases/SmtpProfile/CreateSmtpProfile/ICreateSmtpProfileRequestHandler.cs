namespace UseCases.SmtpProfile.CreateSmtpProfile;

public interface ICreateSmtpProfileRequestHandler
{
    public Task<CreateSmtpProfileResponse> HandleAsync(CreateSmtpProfileRequest request);
}