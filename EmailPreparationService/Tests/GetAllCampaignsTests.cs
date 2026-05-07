using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Models;
using UseCases.Statistics.GetAllCampaigns;

namespace Tests;

public class GetAllCampaignsTests
{
    private AppDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task HandleAsync_WithData_ReturnsSummaries()
    {
        var db = CreateDb();
        var campaignId = Guid.NewGuid();

        await db.OutboxEmails.AddRangeAsync(
            new OutboxEmail { Id = Guid.NewGuid(), CampaignId = campaignId, To = "a@mail.ru", Html = "", Subject = "" },
            new OutboxEmail { Id = Guid.NewGuid(), CampaignId = campaignId, To = "b@mail.ru", Html = "", Subject = "" }
        );
        await db.EmailOpenDatas.AddRangeAsync(
            new EmailOpenData { Id = Guid.NewGuid(), CampaignId = campaignId, Email = "a@mail.ru", OpenedAt = DateTime.UtcNow },
            new EmailOpenData { Id = Guid.NewGuid(), CampaignId = campaignId, Email = "a@mail.ru", OpenedAt = DateTime.UtcNow }
        );
        await db.SaveChangesAsync();

        var handler = new GetAllCampaignsRequestHandler(db);
        var result = await handler.HandleAsync();

        Assert.Single(result.campaignSummaries);
        Assert.Equal(2, result.campaignSummaries[0].totalSent);
        Assert.Equal(1, result.campaignSummaries[0].totalOpened); // уникальных
    }

    [Fact]
    public async Task HandleAsync_EmptyDb_ReturnsEmptyList()
    {
        var db = CreateDb();
        var handler = new GetAllCampaignsRequestHandler(db);

        var result = await handler.HandleAsync();

        Assert.Empty(result.campaignSummaries);
    }

    [Fact]
    public async Task HandleAsync_MultipleCampaigns_ReturnsAllSummaries()
    {
        var db = CreateDb();
        var campaign1 = Guid.NewGuid();
        var campaign2 = Guid.NewGuid();

        await db.OutboxEmails.AddRangeAsync(
            new OutboxEmail { Id = Guid.NewGuid(), CampaignId = campaign1, To = "a@mail.ru", Html = "", Subject = "" },
            new OutboxEmail { Id = Guid.NewGuid(), CampaignId = campaign2, To = "b@mail.ru", Html = "", Subject = "" }
        );
        await db.SaveChangesAsync();

        var handler = new GetAllCampaignsRequestHandler(db);
        var result = await handler.HandleAsync();

        Assert.Equal(2, result.campaignSummaries.Count);
    }
}