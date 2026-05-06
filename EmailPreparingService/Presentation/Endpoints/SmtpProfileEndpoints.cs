using Microsoft.AspNetCore.Mvc;
using UseCases.CreateSmtpProfile;
using UseCases.DeleteSmtpProfile;
using UseCases.GetAllSmtpProfiles;

namespace Presentation.Endpointss;

[ApiController]
[Route("api/smtp")]
public class SmtpProfileEndpoints : ControllerBase
{
    private ICreateSmtpProfileRequestHandler _createSmtpProfileRequestHandler;
    private IDeleteSmtpProfileRequestHandler _deleteSmtpProfileRequestHandler;
    private IGetAllSmtpProfilesRequestHandler _getAllSmtpProfileRequestHandler;
    
    public SmtpProfileEndpoints(ICreateSmtpProfileRequestHandler createSmtpProfileRequestHandler,
        IDeleteSmtpProfileRequestHandler deleteSmtpProfileRequestHandler, IGetAllSmtpProfilesRequestHandler getAllSmtpProfileRequestHandler)
    {
        _createSmtpProfileRequestHandler =  createSmtpProfileRequestHandler;
        _deleteSmtpProfileRequestHandler =  deleteSmtpProfileRequestHandler;
        _getAllSmtpProfileRequestHandler = getAllSmtpProfileRequestHandler;
    }
    
    [HttpPost("")]
    public async Task<IActionResult> CreateSmptProfile([FromBody] CreateSmtpProfileRequest request)
    {
        var respose = await _createSmtpProfileRequestHandler.HandleAsync(request);
        return respose.success ? Ok(respose) : BadRequest(respose);
    }
    
    [HttpDelete("{profileId:guid}")]
    public async Task<IActionResult> DeleteSmptProfile(Guid profileId)
    {
        var response = await _deleteSmtpProfileRequestHandler.HandleAsync(profileId);
        return response ? Ok() : NotFound();
    }

    [HttpGet("")]
    public async Task<IActionResult> GetAllSmtpProfiles()
    {
        Response.Headers["Cache-Control"] = "no-store, no-cache, max-age=0";
        Response.Headers["Pragma"] = "no-cache";
        Response.Headers["Expires"] = "0";

        var response = await _getAllSmtpProfileRequestHandler.HandleAsync();
        return Ok(response);
    }
}
