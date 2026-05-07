namespace UseCases.Statistics.GetCampaign;

public record CampaignInfo(
    Guid campaignId,
    int totalSent,
    int totalOpened,
    double openRate,
    List<RecipientInfo> recipients,
    List<OpenByHour> opensByHour
);