namespace UseCases.Statistics.GetAllCampaigns;

public record GetAllCampaignsResponse(
    List<CampaignSummary> campaignSummaries);