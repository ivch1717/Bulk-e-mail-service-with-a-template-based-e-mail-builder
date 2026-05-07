namespace UseCases.Statistics.GetAllCampaigns;

public interface IGetAllCampaignsRequestHandler
{
    Task<GetAllCampaignsResponse> HandleAsync();
}