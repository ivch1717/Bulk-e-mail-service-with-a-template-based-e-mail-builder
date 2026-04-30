using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace UseCases.GetAllCampaigns;

public interface IGetAllCampaignsRequestHandler
{
    Task<GetAllCampaignsResponse> HandleAsync();
}

public record GetAllCampaignsResponse(
    List<CampaignSummary> campaignSummaries);

public record CampaignSummary(Guid campaignId, int totalSent, int totalOpened);

public class GetAllCampaignsRequestHandler : IGetAllCampaignsRequestHandler
{
    private readonly AppDbContext _db;

    public GetAllCampaignsRequestHandler(AppDbContext db)
    {
        _db = db;
    }
    
    public async Task<GetAllCampaignsResponse> HandleAsync()
    {
        var opened = await _db.EmailOpenDatas
            .GroupBy(e => e.CampaignId)
            .Select(g => new
            {
                CampaignId = g.Key,
                TotalOpened = g.Select(e => e.Email).Distinct().Count()
            })
            .ToListAsync();

        var sent = await _db.OutboxEmails
            .GroupBy(e => e.CampaignId)
            .Select(g => new { CampaignId = g.Key, TotalSent = g.Count() })
            .ToListAsync();

        var summaries = sent.Select(s => new CampaignSummary(
            campaignId: s.CampaignId,
            totalSent: s.TotalSent,
            totalOpened: opened.FirstOrDefault(o => o.CampaignId == s.CampaignId)?.TotalOpened ?? 0
        )).ToList();

        return new GetAllCampaignsResponse(summaries);
    }
}
