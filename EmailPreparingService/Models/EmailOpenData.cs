namespace Models;

public class EmailOpenData
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    public string Email { get; set; }
    public DateTime OpenedAt { get; set; }
    public string? UserAgent { get; set; }
}