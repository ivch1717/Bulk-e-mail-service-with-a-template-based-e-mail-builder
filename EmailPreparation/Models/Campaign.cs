namespace Models;

/// <summary>
/// Информация о рассылке.
/// </summary>
public class Campaign
{
    public Guid CampaignId { get; init; }
    
    public string CampaignName { get; init; } = "";
}