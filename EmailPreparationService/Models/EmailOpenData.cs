namespace Models;

/// <summary>
/// Информация об открытии письма.
/// </summary>
public class EmailOpenData
{
    public Guid Id { get; init; }
    public Guid CampaignId { get; init; }
    public string Email { get; init; } = "";
    public DateTime OpenedAt { get; init; }
    public string UserAgent { get; init; } = "";
}