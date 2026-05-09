namespace Models;

/// <summary>
/// Профиль SMTP.
/// </summary>
public class SmtpProfile
{
    public Guid Id { get; init; }
    public string DisplayEmail { get; set; } = "";
    public string DisplayName { get; set; } = "";
}