using Microsoft.AspNetCore.Mvc;
using UseCases.CreateSmtpProfile;
using UseCases.DeleteSmtpProfile;
using UseCases.GetAllSmtpProfiles;

namespace Presentation.Endpoints;

[ApiController]
[Route("api/smtp")]
public class SmtpProfileEndpoints(
    ICreateSmtpProfileRequestHandler createSmtpProfileRequestHandler,
    IDeleteSmtpProfileRequestHandler deleteSmtpProfileRequestHandler,
    IGetAllSmtpProfilesRequestHandler getAllSmtpProfileRequestHandler)
    : ControllerBase
{
    [HttpPost("")]
    public async Task<IActionResult> CreateSmtpProfile([FromBody] CreateSmtpProfileRequest request)
    {
        var response = await createSmtpProfileRequestHandler.HandleAsync(request);
        return response.success ? Ok(response) : BadRequest(response);
    }
    
    [HttpDelete("{profileId:guid}")]
    public async Task<IActionResult> DeleteSmtpProfile(Guid profileId)
    {
        var response = await deleteSmtpProfileRequestHandler.HandleAsync(profileId);
        return response ? Ok() : NotFound();
    }

    [HttpGet("")]
    public async Task<IActionResult> GetAllSmtpProfiles()
    {
        Response.Headers["Cache-Control"] = "no-store, no-cache, max-age=0";
        Response.Headers["Pragma"] = "no-cache";
        Response.Headers["Expires"] = "0";

        var response = await getAllSmtpProfileRequestHandler.HandleAsync();
        return Ok(response);
    }
}
