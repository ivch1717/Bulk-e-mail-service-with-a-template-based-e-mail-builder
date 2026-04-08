using Microsoft.AspNetCore.Mvc;
using UseCases;
using UseCases.ExtractTableHeaders;

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
    
    public Endpoints( IUploadTemplateRequestHandler uploadTemplateRequestHandler, IProcessEmailCreationRequestHandler processEmailCreationRequestHandler,
        IExtractTableHeadersRequestHandler extractTableHeadersRequestHandler)
        IExtractTableHeadersRequestHandler extractTableHeadersRequestHandler, IGetPreviewRequestHandler getPreviewRequestHandler,
    {
        // _uploadDataRequestHandler = uploadDataRequestHandler;
        _uploadTemplateRequestHandler = uploadTemplateRequestHandler;
        _processEmailCreationRequestHandler = processEmailCreationRequestHandler;
        _extractTableHeadersRequestHandler = extractTableHeadersRequestHandler;
        _getPreviewRequestHandler = getPreviewRequestHandler;
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
        return Ok(_uploadTemplateRequestHandler.Handle(request));
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
    /// <returns>Заголовки в виде списка строк</returns>
    [HttpPost("api/ExtractTableHeaders")]
    public IActionResult ExtractTableHeaders([FromForm] ExtractTableHeadersRequest request)
    {
        return Ok(_extractTableHeadersRequestHandler.Handle(request));
    }
}