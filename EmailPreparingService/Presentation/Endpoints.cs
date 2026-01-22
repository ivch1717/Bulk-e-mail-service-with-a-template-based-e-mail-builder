using Microsoft.AspNetCore.Mvc;
using UseCases;

namespace Presentation;

[ApiController]
[Route("")]
public class Endpoints : ControllerBase
{
    IUploadDataRequestHandler _uploadDataRequestHandler;
    IUploadTemplateRequestHandler _uploadTemplateRequestHandler;
    
    public Endpoints(IUploadDataRequestHandler  uploadDataRequestHandler, IUploadTemplateRequestHandler uploadTemplateRequestHandler)
    {
        _uploadDataRequestHandler = uploadDataRequestHandler;
        _uploadTemplateRequestHandler = uploadTemplateRequestHandler;
    }
    
    [HttpPost("UploadData")]
    public IActionResult UploadData([FromForm] UploadDataRequest request)
    {
        return Ok(_uploadDataRequestHandler.Handle(request));
    }

    /// <summary>
    /// Загрузка html шаблона письма, для обнаружения подстановочных переменных.
    /// </summary>
    /// <param name="request">html шаблон.</param>
    /// <returns>Список названий подстановочных переменных, обнаруженных в шаблоне.</returns>
    [HttpPost("UploadTemplate")]
    public IActionResult UploadTemplate([FromForm] UploadTemplateRequest request)
    {
        return Ok(_uploadTemplateRequestHandler.Handle(request));
    }
    
    
}