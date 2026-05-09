namespace Models;

/// <summary>
/// Полная информация о письме для рассылки.
/// </summary>
public class OutboxEmail
{
    public Guid Id { get; init; }
    public Guid SmtpId { get; init; }
    public string To { get; init; } = "";
    public string Html { get; init; } = "";
    public DateTime CreatedAt { get; init; }
    public bool Sent { get; init; }
    public Guid CampaignId { get; init; }
    public string Subject { get; init; } = "";
}