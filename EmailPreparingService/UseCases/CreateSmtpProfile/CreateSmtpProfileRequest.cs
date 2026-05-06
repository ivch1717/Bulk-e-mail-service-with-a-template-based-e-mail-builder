using Infrastructure;
using Models;

namespace UseCases.CreateSmtpProfile;

public record CreateSmtpProfileRequest(
    string host,
    int port,
    string user,
    string password,
    string fromEmail,
    string displayName
);

public record CreateSmtpProfileResponse(
    bool success
);

public interface ICreateSmtpProfileRequestHandler
{
    public Task<CreateSmtpProfileResponse> HandleAsync(CreateSmtpProfileRequest request);
}

public class CreateSmtpProfileRequestHandler : ICreateSmtpProfileRequestHandler
{
    private readonly IMailSenderClient _client;
    private readonly AppDbContext _db;
    
    public CreateSmtpProfileRequestHandler(IMailSenderClient mailSenderClient, AppDbContext dbContext)
    {
        _client = mailSenderClient;
        _db = dbContext;
    }
    
    public async Task<CreateSmtpProfileResponse> HandleAsync(CreateSmtpProfileRequest request)
    {
        Guid? createdProfileId = null;

        try
        {
            var response = await _client.CreateSmtpProfileAsync(request);
            createdProfileId = response;
            await _db.SmtpProfiles.AddAsync(new SmtpProfile{Id = response, DisplayEmail = request.fromEmail, DisplayName = request.displayName});
            await _db.SaveChangesAsync();
            return new CreateSmtpProfileResponse(true);
        }
        catch (Exception)
        {
            if (createdProfileId is { } profileId)
            {
                try
                {
                    await _client.DeleteSmtpProfileAsync(profileId);
                }
                catch
                {
                }
            }

            return new CreateSmtpProfileResponse(false);
        }
    }
}
