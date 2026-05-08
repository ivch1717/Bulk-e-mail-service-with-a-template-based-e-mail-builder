using Microsoft.AspNetCore.Mvc;
using UseCases.Statistics.GetAllCampaigns;
using UseCases.Statistics.GetCampaign;
using UseCases.Statistics.TrackOpen;

namespace Presentation.Endpoints;

[ApiController]
[Route("api")]
public class StatisticsEndpoints(
    ITrackOpenRequestHandler trackOpenRequestHandler,
    IGetAllCampaignsRequestHandler getAllCampaignsRequestHandler,
    IGetCampaignRequestHandler getCampaignRequestHandler)
    : ControllerBase
{
    [HttpGet("track/open")]
    public async Task<IActionResult> TrackOpen([FromQuery] TrackOpenRequest request)
    {
        var userAgent = Request.Headers["User-Agent"].ToString();
        request = request with { UserAgent = userAgent };
        var pixel = await trackOpenRequestHandler.HandleAsync(request);
        return File(pixel, "image/gif");
    }

    /// <summary>
    /// Получение информации о всех рассылках.
    /// </summary>
    /// <returns>Список краткой информации о каждой рассылке.</returns>
    [HttpGet("stats/campaigns")]
    public async Task<IActionResult> GetAllCampaigns()
    {
        var response = await getAllCampaignsRequestHandler.HandleAsync();
        return Ok(response);
    }
    
    /// <summary>
    /// Получение информации о конкретной рассылке.
    /// </summary>
    /// <returns>Полная информация о каждой рассылке.</returns>
    [HttpGet("stats/campaigns/{campaignId}")]
    public async Task<IActionResult> GetCampaign(Guid campaignId)
    {
        var response = await getCampaignRequestHandler.HandleAsync(campaignId);
        return Ok(response);
    }
}
