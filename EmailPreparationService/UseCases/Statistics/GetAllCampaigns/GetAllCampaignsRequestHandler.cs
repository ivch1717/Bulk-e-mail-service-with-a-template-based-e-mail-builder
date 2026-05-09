using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace UseCases.Statistics.GetAllCampaigns;

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

        var summaries = await _db.OutboxEmails
            .GroupBy(e => e.CampaignId)
            .Select(g => new
            {
                CampaignId = g.Key,
                TotalSent = g.Count(),
                CampaignName = _db.Campaigns
                    .Where(c => c.CampaignId == g.Key)
                    .Select(c => c.CampaignName)
                    .FirstOrDefault()
            })
            .ToListAsync();

        var result = summaries.Select(s => new CampaignSummary(
            campaignId: s.CampaignId,
            totalSent: s.TotalSent,
            totalOpened: opened.FirstOrDefault(o => o.CampaignId == s.CampaignId)?.TotalOpened ?? 0,
            campaignName: s.CampaignName
        )).ToList();

        return new GetAllCampaignsResponse(result);
    }
}