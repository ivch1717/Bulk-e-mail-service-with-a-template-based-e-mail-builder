using Microsoft.AspNetCore.Mvc;

namespace UseCases.Statistics.TrackOpen;

public record TrackOpenRequest(
    [FromQuery] Guid CampaignId,
    [FromQuery] string Email,
    string? UserAgent
);