using Microsoft.AspNetCore.Mvc;
using UseCases;

namespace Presentation;

[ApiController]
[Route("")]
public class Endpoints : ControllerBase
{
    // IUploadDataRequestHandler _uploadDataRequestHandler;
    IUploadTemplateRequestHandler _uploadTemplateRequestHandler;
    IProcessEmailCreationRequestHandler _processEmailCreationRequestHandler;
    
    public Endpoints( IUploadTemplateRequestHandler uploadTemplateRequestHandler, IProcessEmailCreationRequestHandler processEmailCreationRequestHandler )
    {
        // _uploadDataRequestHandler = uploadDataRequestHandler;
        _uploadTemplateRequestHandler = uploadTemplateRequestHandler;
        _processEmailCreationRequestHandler = processEmailCreationRequestHandler;
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

    [HttpPost("ProcessEmailCreation")]
    public IActionResult ProcessEmailCreation([FromForm] ProcessEmailCreationRequest request)
    {
        return Ok(_processEmailCreationRequestHandler.Handle(request));
    }
}