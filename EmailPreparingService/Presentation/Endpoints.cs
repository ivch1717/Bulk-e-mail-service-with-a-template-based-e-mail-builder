using Microsoft.AspNetCore.Mvc;
using UseCases;
using UseCases.ExtractTableHeaders;
using UseCases.GetPreview;

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
    
    public Endpoints( IUploadTemplateRequestHandler uploadTemplateRequestHandler, IProcessEmailCreationRequestHandler processEmailCreationRequestHandler,
        IExtractTableHeadersRequestHandler extractTableHeadersRequestHandler, IGetPreviewRequestHandler getPreviewRequestHandler,
        ISendRequestHandler sendRequestHandler, ITrackOpenRequestHandler trackOpenRequestHandler)
    {
        // _uploadDataRequestHandler = uploadDataRequestHandler;
        _uploadTemplateRequestHandler = uploadTemplateRequestHandler;
        _processEmailCreationRequestHandler = processEmailCreationRequestHandler;
        _extractTableHeadersRequestHandler = extractTableHeadersRequestHandler;
        _getPreviewRequestHandler = getPreviewRequestHandler;
        _sendRequestHandler = sendRequestHandler;
        _trackOpenRequestHandler =  trackOpenRequestHandler;
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
            return Ok(_uploadTemplateRequestHandler.Handle(request));
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
        return Ok(await _sendRequestHandler.Handle(request));
    }

    [HttpGet("api/track/open")]
    public async Task<IActionResult> TrackOpen([FromQuery] TrackOpenRequest request)
    {
        var pixel = await _trackOpenRequestHandler.HandleAsync(request);
        return File(pixel, "image/gif");
    }
}