using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Models;
using Moq;
using UseCases.GetAllSmtpProfiles;

namespace Tests;

public class GetAllSmtpProfilesTests
{
    private readonly Mock<IMailSenderClient> _client = new();

    private AppDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task HandleAsync_ClientReturnsProfiles_ReturnsOrderedList()
    {
        var db = CreateDb();
        _client.Setup(c => c.GetSmtpProfilesAsync()).ReturnsAsync([
            new MailSenderSmtpProfile(Guid.NewGuid(), "user1", "b@mail.ru", "Beta"),
            new MailSenderSmtpProfile(Guid.NewGuid(), "user2", "a@mail.ru", "Alpha")
        ]);

        var handler = new GetAllSmtpProfilesRequestHandler(db, _client.Object);
        var result = await handler.HandleAsync();

        Assert.Equal(2, result.smtpProfiles.Count);
        Assert.Equal("Alpha", result.smtpProfiles[0].displayName);
        Assert.Equal("Beta", result.smtpProfiles[1].displayName);
    }

    [Fact]
    public async Task HandleAsync_ClientThrows_FallsBackToDb()
    {
        var db = CreateDb();
        await db.SmtpProfiles.AddAsync(new SmtpProfile
        {
            Id = Guid.NewGuid(),
            DisplayEmail = "test@mail.ru",
            DisplayName = "Test"
        });
        await db.SaveChangesAsync();

        _client.Setup(c => c.GetSmtpProfilesAsync()).ThrowsAsync(new Exception());

        var handler = new GetAllSmtpProfilesRequestHandler(db, _client.Object);
        var result = await handler.HandleAsync();

        Assert.Single(result.smtpProfiles);
        Assert.Equal("Test", result.smtpProfiles[0].displayName);
    }

    [Fact]
    public async Task HandleAsync_ClientReturnsProfiles_SyncsToDb()
    {
        var db = CreateDb();
        var profileId = Guid.NewGuid();
        _client.Setup(c => c.GetSmtpProfilesAsync()).ReturnsAsync([
            new MailSenderSmtpProfile(profileId, "user1", "test@mail.ru", "Test")
        ]);

        var handler = new GetAllSmtpProfilesRequestHandler(db, _client.Object);
        await handler.HandleAsync();

        Assert.Equal(1, await db.SmtpProfiles.CountAsync());
        Assert.Equal(profileId, (await db.SmtpProfiles.FirstAsync()).Id);
    }
}