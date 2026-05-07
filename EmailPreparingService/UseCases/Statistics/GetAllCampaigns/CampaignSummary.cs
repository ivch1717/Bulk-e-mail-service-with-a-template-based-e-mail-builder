namespace UseCases.Statistics.GetAllCampaigns;

public record CampaignSummary(Guid campaignId, int totalSent, int totalOpened);