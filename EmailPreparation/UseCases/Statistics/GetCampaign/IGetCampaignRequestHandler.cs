namespace UseCases.Statistics.GetCampaign;

public interface IGetCampaignRequestHandler
{
    Task<CampaignInfo> HandleAsync(Guid campaignId);
}