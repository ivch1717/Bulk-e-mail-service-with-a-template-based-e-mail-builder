namespace UseCases.SmtpProfile.GetAllSmtpProfiles;

public interface IGetAllSmtpProfilesRequestHandler
{
    public Task<GetAllSmtpProfilesResponse> HandleAsync();
}