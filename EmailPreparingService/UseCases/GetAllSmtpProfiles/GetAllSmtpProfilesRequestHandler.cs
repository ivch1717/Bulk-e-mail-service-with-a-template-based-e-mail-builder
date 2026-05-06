using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Models;

namespace UseCases.GetAllSmtpProfiles;

public record SmtpProfileListItem(Guid id, string user, string fromEmail, string displayName);

public record GetAllSmtpProfilesResponse(List<SmtpProfileListItem> smtpProfiles);

public interface IGetAllSmtpProfilesRequestHandler
{
    public Task<GetAllSmtpProfilesResponse> HandleAsync();
}

public class GetAllSmtpProfilesRequestHandler : IGetAllSmtpProfilesRequestHandler
{
    private readonly AppDbContext _db;
    private readonly IMailSenderClient _mailSenderClient;
    
    public GetAllSmtpProfilesRequestHandler(AppDbContext db, IMailSenderClient mailSenderClient)
    {
        _db = db;
        _mailSenderClient = mailSenderClient;
    }
    
    public async Task<GetAllSmtpProfilesResponse> HandleAsync()
    {
        try
        {
            var mailSenderProfiles = await _mailSenderClient.GetSmtpProfilesAsync();
            await SyncPublicProfilesAsync(mailSenderProfiles);

            var profiles = mailSenderProfiles
                .OrderBy(profile => profile.DisplayName)
                .ThenBy(profile => profile.FromEmail)
                .Select(profile => new SmtpProfileListItem(
                    profile.Id,
                    profile.User,
                    profile.FromEmail,
                    profile.DisplayName))
                .ToList();

            return new GetAllSmtpProfilesResponse(profiles);
        }
        catch
        {
            var profiles = await _db.SmtpProfiles
                .OrderBy(profile => profile.DisplayName)
                .Select(profile => new SmtpProfileListItem(
                    profile.Id,
                    "",
                    profile.DisplayEmail,
                    profile.DisplayName))
                .ToListAsync();

            return new GetAllSmtpProfilesResponse(profiles);
        }
    }

    private async Task SyncPublicProfilesAsync(IReadOnlyList<MailSenderSmtpProfile> mailSenderProfiles)
    {
        var ids = mailSenderProfiles.Select(profile => profile.Id).ToArray();
        var existing = await _db.SmtpProfiles
            .Where(profile => ids.Contains(profile.Id))
            .ToDictionaryAsync(profile => profile.Id);

        foreach (var profile in mailSenderProfiles)
        {
            if (existing.TryGetValue(profile.Id, out var publicProfile))
            {
                publicProfile.DisplayEmail = profile.FromEmail;
                publicProfile.DisplayName = profile.DisplayName;
                continue;
            }

            await _db.SmtpProfiles.AddAsync(new SmtpProfile
            {
                Id = profile.Id,
                DisplayEmail = profile.FromEmail,
                DisplayName = profile.DisplayName
            });
        }

        await _db.SaveChangesAsync();
    }
}
