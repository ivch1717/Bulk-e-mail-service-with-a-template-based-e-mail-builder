using Infrastructure;
using Models;

namespace UseCases.DeleteSmtpProfile;

public interface IDeleteSmtpProfileRequestHandler
{
    public Task<bool> HandleAsync(Guid profileId);
}

public class DeleteSmtpProfileRequestHandler : IDeleteSmtpProfileRequestHandler
{
    private readonly IMailSenderClient _client;
    private readonly AppDbContext _db;
    
    public DeleteSmtpProfileRequestHandler(IMailSenderClient mailSenderClient, AppDbContext dbContext)
    {
        _client = mailSenderClient;
        _db = dbContext;
    }
    
    public async Task<bool> HandleAsync(Guid profileId)
    {
        try
        {
            var profile = await _db.SmtpProfiles.FindAsync(profileId);
            if (profile == null)
            {
                return false;
            }
            await _client.DeleteSmtpProfileAsync(profileId);
            _db.SmtpProfiles.Remove(profile);
            await _db.SaveChangesAsync();
        
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}