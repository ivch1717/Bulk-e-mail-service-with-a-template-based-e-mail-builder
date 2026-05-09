using Infrastructure;
using Models;

namespace UseCases.Statistics.TrackOpen;

public class TrackOpenRequestHandler : ITrackOpenRequestHandler
{
    private readonly AppDbContext _dbContext;

    public TrackOpenRequestHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<byte[]> HandleAsync(TrackOpenRequest request)
    {
        var record = new EmailOpenData
        {
            Id = Guid.NewGuid(),
            CampaignId = request.CampaignId,
            Email = request.Email,
            OpenedAt = DateTime.UtcNow,
            UserAgent = request.UserAgent,
        };

        await _dbContext.EmailOpenDatas.AddAsync(record);
        await _dbContext.SaveChangesAsync();

        return Convert.FromBase64String("R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7");
    }
}