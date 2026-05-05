using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Models;

namespace UseCases.GetAllSmtpProfiles;

public record GetAllSmtpProfilesResponse(List<SmtpProfile> smtpProfiles);

public interface IGetAllSmtpProfilesRequestHandler
{
    public Task<GetAllSmtpProfilesResponse> HandleAsync();
}

public class GetAllSmtpProfilesRequestHandler : IGetAllSmtpProfilesRequestHandler
{
    private readonly AppDbContext _db;
    
    public GetAllSmtpProfilesRequestHandler(AppDbContext db)
    {
        _db = db;
    }
    
    public async Task<GetAllSmtpProfilesResponse> HandleAsync()
    {
        var profiles = await _db.SmtpProfiles.ToListAsync();
        return new GetAllSmtpProfilesResponse(profiles);
    }
}