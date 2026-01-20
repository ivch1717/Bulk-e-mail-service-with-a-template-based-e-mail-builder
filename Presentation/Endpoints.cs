using Microsoft.AspNetCore.Mvc;
using UseCases;

namespace Presentation;

[ApiController]
[Route("")]
public class Endpoints : ControllerBase
{
    IUploadDataRequestHandler _uploadDataRequestHandler;
    
    public Endpoints(IUploadDataRequestHandler  uploadDataRequestHandler)
    {
        _uploadDataRequestHandler = uploadDataRequestHandler;
    }
    
    [HttpPost("UploadData")]
    public IActionResult UploadData([FromForm] UploadDataRequest request)
    {
        return Ok(_uploadDataRequestHandler.Handle(request));
    }
}