using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace UseCases.GetCampaign;

public interface IGetCampaignRequestHandler
{
    Task<CampaignInfo> HandleAsync(Guid campaignId);
}

public record RecipientInfo(string email, DateTime openedAt, string? userAgent);

public record OpenByHour(DateTime hour, int count);

public record CampaignInfo(
    Guid campaignId,
    int totalSent,
    int totalOpened,
    double openRate,
    List<RecipientInfo> recipients,
    List<OpenByHour> opensByHour
);

public class GetCampaignRequestHandler : IGetCampaignRequestHandler
{
    private readonly AppDbContext _db;

    public GetCampaignRequestHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<CampaignInfo> HandleAsync(Guid campaignId)
    {
        var totalSent = await _db.OutboxEmails
            .CountAsync(e => e.CampaignId == campaignId);

        var opens = await _db.EmailOpenDatas
            .Where(e => e.CampaignId == campaignId)
            .ToListAsync();

        var totalOpened = opens.Select(e => e.Email).Distinct().Count();
        var openRate = totalSent == 0 ? 0 : Math.Round((double)totalOpened / totalSent * 100, 1);

        var recipients = opens
            .GroupBy(e => e.Email)
            .Select(g => new RecipientInfo(
                email: g.Key,
                openedAt: g.Min(e => e.OpenedAt),
                userAgent: g.First().UserAgent
            ))
            .ToList();

        var opensByHourDict = opens
            .GroupBy(e => e.OpenedAt.Hour)
            .ToDictionary(g => g.Key, g => g.Count());

        var opensByHour = Enumerable.Range(0, 24)
            .Select(h => new OpenByHour(
                hour: new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, h, 0, 0),
                count: opensByHourDict.GetValueOrDefault(h, 0)
            ))
            .ToList();

        return new CampaignInfo(campaignId, totalSent, totalOpened, openRate, recipients, opensByHour);
    }
}