using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Models;
using Moq;
using UseCases.MailSender;
using UseCases.SmtpProfile.DeleteSmtpProfile;

namespace Tests;

public class DeleteSmtpProfileTests
{
    private readonly Mock<IMailSenderClient> _client = new();

    private AppDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task HandleAsync_ExistingProfile_DeletesAndReturnsTrue()
    {
        var db = CreateDb();
        var profileId = Guid.NewGuid();
        await db.SmtpProfiles.AddAsync(new SmtpProfile { Id = profileId, DisplayEmail = "test@mail.ru", DisplayName = "Test" });
        await db.SaveChangesAsync();

        _client.Setup(c => c.DeleteSmtpProfileAsync(profileId)).Returns(Task.CompletedTask);

        var handler = new DeleteSmtpProfileRequestHandler(_client.Object, db);
        var result = await handler.HandleAsync(profileId);

        Assert.True(result);
        Assert.Equal(0, await db.SmtpProfiles.CountAsync());
        _client.Verify(c => c.DeleteSmtpProfileAsync(profileId), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_NonExistingProfile_ReturnsFalse()
    {
        var db = CreateDb();
        var handler = new DeleteSmtpProfileRequestHandler(_client.Object, db);

        var result = await handler.HandleAsync(Guid.NewGuid());

        Assert.False(result);
        _client.Verify(c => c.DeleteSmtpProfileAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ClientThrows_ReturnsFalse()
    {
        var db = CreateDb();
        var profileId = Guid.NewGuid();
        await db.SmtpProfiles.AddAsync(new SmtpProfile { Id = profileId, DisplayEmail = "test@mail.ru", DisplayName = "Test" });
        await db.SaveChangesAsync();

        _client.Setup(c => c.DeleteSmtpProfileAsync(profileId)).ThrowsAsync(new Exception());

        var handler = new DeleteSmtpProfileRequestHandler(_client.Object, db);
        var result = await handler.HandleAsync(profileId);

        Assert.False(result);
        Assert.Equal(1, await db.SmtpProfiles.CountAsync());
    }
}