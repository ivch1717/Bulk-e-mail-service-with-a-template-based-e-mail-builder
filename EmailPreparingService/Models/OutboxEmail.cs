namespace Models;

public class OutboxEmail
{
    public Guid Id { get; set; }
    public string To { get; set; } = "";
    public string Html { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public bool Sent { get; set; } = false;
<<<<<<< Updated upstream
=======
    
    public Guid CampaignId { get; set; }
    
    public string Subject { get; set; }
>>>>>>> Stashed changes
}