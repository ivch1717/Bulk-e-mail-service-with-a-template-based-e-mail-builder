using Microsoft.AspNetCore.Mvc;
using UseCases;
using UseCases.CreateSmtpProfile;
using UseCases.DeleteSmtpProfile;
using UseCases.ExtractTableHeaders;
using UseCases.GetAllCampaigns;
using UseCases.GetAllSmtpProfiles;
using UseCases.GetCampaign;
using UseCases.GetPreview;
using UseCases.UploadTemplate;

namespace Presentation;

[ApiController]
[Route("")]
public class Endpoints : ControllerBase
{
    // IUploadDataRequestHandler _uploadDataRequestHandler;
    IUploadTemplateRequestHandler _uploadTemplateRequestHandler;
    IProcessEmailCreationRequestHandler _processEmailCreationRequestHandler;
    IExtractTableHeadersRequestHandler _extractTableHeadersRequestHandler;
    IGetPreviewRequestHandler _getPreviewRequestHandler;
    ISendRequestHandler _sendRequestHandler;
    private ITrackOpenRequestHandler _trackOpenRequestHandler;
    private IGetAllCampaignsRequestHandler _getAllCampaignsRequestHandler;
    private IGetCampaignRequestHandler _getCampaignRequestHandler;
    private ICreateSmtpProfileRequestHandler _createSmtpProfileRequestHandler;
    private IDeleteSmtpProfileRequestHandler _deleteSmtpProfileRequestHandler;
    private IGetAllSmtpProfilesRequestHandler _getAllSmtpProfileRequestHandler;
    
    public Endpoints( IUploadTemplateRequestHandler uploadTemplateRequestHandler, IProcessEmailCreationRequestHandler processEmailCreationRequestHandler,
        IExtractTableHeadersRequestHandler extractTableHeadersRequestHandler, IGetPreviewRequestHandler getPreviewRequestHandler,
        ISendRequestHandler sendRequestHandler, ITrackOpenRequestHandler trackOpenRequestHandler, IGetAllCampaignsRequestHandler getAllCampaignsRequestHandler,
        IGetCampaignRequestHandler getCampaignRequestHandler, ICreateSmtpProfileRequestHandler createSmtpProfileRequestHandler,
        IDeleteSmtpProfileRequestHandler deleteSmtpProfileRequestHandler, IGetAllSmtpProfilesRequestHandler getAllSmtpProfileRequestHandler)
    {
        // _uploadDataRequestHandler = uploadDataRequestHandler;
        _uploadTemplateRequestHandler = uploadTemplateRequestHandler;
        _processEmailCreationRequestHandler = processEmailCreationRequestHandler;
        _extractTableHeadersRequestHandler = extractTableHeadersRequestHandler;
        _getPreviewRequestHandler = getPreviewRequestHandler;
        _sendRequestHandler = sendRequestHandler;
        _trackOpenRequestHandler =  trackOpenRequestHandler;
        _getAllCampaignsRequestHandler = getAllCampaignsRequestHandler;
        _getCampaignRequestHandler = getCampaignRequestHandler;
        _createSmtpProfileRequestHandler =  createSmtpProfileRequestHandler;
        _deleteSmtpProfileRequestHandler =  deleteSmtpProfileRequestHandler;
        _getAllSmtpProfileRequestHandler = getAllSmtpProfileRequestHandler;
    }
    
    // [HttpPost("UploadData")]
    // public IActionResult UploadData([FromForm] UploadDataRequest request)
    // {
    //     return Ok(_uploadDataRequestHandler.Handle(request));
    // }

    /// <summary>
    /// Загрузка html шаблона письма, для обнаружения подстановочных переменных.
    /// </summary>
    /// <param name="request">html шаблон.</param>
    /// <returns>Список названий подстановочных переменных, обнаруженных в шаблоне.</returns>
    [HttpPost("api/UploadTemplate")]
    public IActionResult UploadTemplate([FromForm] UploadTemplateRequest request)
    {
        try
        {
            var response = _uploadTemplateRequestHandler.Handle(request);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return BadRequest("Unknown exception");
        }
    }

    [HttpPost("api/ProcessEmailCreation")]
    public IActionResult ProcessEmailCreation([FromForm] ProcessEmailCreationRequest request)
    {
        return Ok(_processEmailCreationRequestHandler.Handle(request));
    }

    /// <summary>
    /// Получение всех заголовков столбцов из таблицы.
    /// Заголовки берутся из первой не пустой строки таблицы.
    /// Формат таблицы .xlsx.
    /// </summary>
    /// <param name="request">.xlsx таблица.</param>
    /// <returns>Заголовки в виде списка строк, если нет заголовков то код 422.</returns>
    [HttpPost("api/ExtractTableHeaders")]
    public IActionResult ExtractTableHeaders([FromForm] ExtractTableHeadersRequest request)
    {
        try
        {
            var response = _extractTableHeadersRequestHandler.Handle(request);
            return response.headers.Count == 0
                ? UnprocessableEntity("There are no headers in the table")
                : Ok(response.headers);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return BadRequest("Unknown exception");
        }
    }
    
    /// <summary>
    /// Получение определенного числа писем для предпросмотра на сайте.
    /// </summary>
    /// <param name="request">.xlsx таблица,
    /// .html шаблон,
    /// int строка с таблицы с которой нужно начать,
    /// int количество писем которые нужно сгенерировать,
    /// map переменных шаблона со столбцами таблицы.</param>
    /// <returns>Список писем с адресатами и номер строки следующей за той, что была обработана последней.</returns>
    [HttpPost("api/GetPreview")]
    public IActionResult GetPreview([FromForm] GetPreviewRequest request)
    {
        return Ok(_getPreviewRequestHandler.Handle(request));
    }
    
    
    [HttpPost("api/Send")]
    public async Task<IActionResult> Send([FromForm] SendRequest request)
    {
        try
        {
            return Ok(await _sendRequestHandler.Handle(request));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
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

    [HttpPost("api/smtp")]
    public async Task<IActionResult> CreateSmptProfile([FromBody] CreateSmtpProfileRequest request)
    {
        var respose = await _createSmtpProfileRequestHandler.HandleAsync(request);
        return respose.success ? Ok(respose) : BadRequest(respose);
    }
    
    [HttpDelete("api/smtp/{profileId:guid}")]
    public async Task<IActionResult> DeleteSmptProfile(Guid profileId)
    {
        var response = await _deleteSmtpProfileRequestHandler.HandleAsync(profileId);
        return response ? Ok() : NotFound();
    }

    [HttpGet("api/smtp")]
    public async Task<IActionResult> GetAllSmtpProfiles()
    {
        Response.Headers["Cache-Control"] = "no-store, no-cache, max-age=0";
        Response.Headers["Pragma"] = "no-cache";
        Response.Headers["Expires"] = "0";

        var response = await _getAllSmtpProfileRequestHandler.HandleAsync();
        return Ok(response);
    }
}
