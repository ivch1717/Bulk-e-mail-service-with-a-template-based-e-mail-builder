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

    [HttpPost("UploadTemplate")]
    public IActionResult UploadTemplate([FromForm] UploadTemplateRequest request)
    {
        return Ok(_uploadTemplateRequestHandler.Handle(request));
    }
}