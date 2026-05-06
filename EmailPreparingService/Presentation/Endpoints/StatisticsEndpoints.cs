using Microsoft.AspNetCore.Mvc;
using UseCases.GetAllCampaigns;
using UseCases.GetCampaign;

namespace Presentation;

[ApiController]
[Route("")]
public class StatisticsEndpoints : ControllerBase
{
    private ITrackOpenRequestHandler _trackOpenRequestHandler;
    private IGetAllCampaignsRequestHandler _getAllCampaignsRequestHandler;
    private IGetCampaignRequestHandler _getCampaignRequestHandler;
    
    public StatisticsEndpoints( ITrackOpenRequestHandler trackOpenRequestHandler, IGetAllCampaignsRequestHandler getAllCampaignsRequestHandler,
        IGetCampaignRequestHandler getCampaignRequestHandler)
    {
        _trackOpenRequestHandler =  trackOpenRequestHandler;
        _getAllCampaignsRequestHandler = getAllCampaignsRequestHandler;
        _getCampaignRequestHandler = getCampaignRequestHandler;
    }
    
    [HttpGet("api/track/open")]
    public async Task<IActionResult> TrackOpen([FromQuery] TrackOpenRequest request)
    {
        var pixel = await _trackOpenRequestHandler.HandleAsync(request);
        return File(pixel, "image/gif");
    }

    /// <summary>
    /// Получение информации о всех рассылках.
    /// </summary>
    /// <returns>Список краткой информации о каждой рассылке.</returns>
    [HttpGet("api/stats/campaigns")]
    public async Task<IActionResult> GetAllCampaigns()
    {
        var response = await _getAllCampaignsRequestHandler.HandleAsync();
        return Ok(response);
    }
    
    /// <summary>
    /// Получение информации о конкретной рассылке.
    /// </summary>
    /// <returns>Полная информация о каждой рассылке.</returns>
    [HttpGet("api/stats/campaigns/{campaignId}")]
    public async Task<IActionResult> GetCampaign(Guid campaignId)
    {
        var response = await _getCampaignRequestHandler.HandleAsync(campaignId);
        return Ok(response);
    }
}
