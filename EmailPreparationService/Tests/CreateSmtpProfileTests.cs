using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Moq;
using UseCases.MailSender;
using UseCases.SmtpProfile.CreateSmtpProfile;

namespace Tests;

public class CreateSmtpProfileTests
{
    private readonly Mock<IMailSenderClient> _client = new();

    private AppDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task HandleAsync_Success_ReturnsTrueAndSavesToDb()
    {
        var db = CreateDb();
        var profileId = Guid.NewGuid();
        _client.Setup(c => c.CreateSmtpProfileAsync(It.IsAny<CreateSmtpProfileRequest>()))
            .ReturnsAsync(profileId);

        var handler = new CreateSmtpProfileRequestHandler(_client.Object, db);
        var result = await handler.HandleAsync(new CreateSmtpProfileRequest(
            "smtp.yandex.ru", 587, "user", "pass", "from@mail.ru", "Test"));

        Assert.True(result.success);
        Assert.Equal(1, await db.SmtpProfiles.CountAsync());
    }

    [Fact]
    public async Task HandleAsync_ClientThrows_ReturnsFalse()
    {
        var db = CreateDb();
        _client.Setup(c => c.CreateSmtpProfileAsync(It.IsAny<CreateSmtpProfileRequest>()))
            .ThrowsAsync(new Exception("connection failed"));

        var handler = new CreateSmtpProfileRequestHandler(_client.Object, db);
        var result = await handler.HandleAsync(new CreateSmtpProfileRequest(
            "smtp.yandex.ru", 587, "user", "pass", "from@mail.ru", "Test"));

        Assert.False(result.success);
        Assert.Equal(0, await db.SmtpProfiles.CountAsync());
    }

    [Fact]
    public async Task HandleAsync_DbThrows_DeletesProfileFromMailSender()
    {
        var db = CreateDb();
        var profileId = Guid.NewGuid();
        _client.Setup(c => c.CreateSmtpProfileAsync(It.IsAny<CreateSmtpProfileRequest>()))
            .ReturnsAsync(profileId);
        _client.Setup(c => c.DeleteSmtpProfileAsync(profileId))
            .Returns(Task.CompletedTask);

        // сломаем DbContext чтобы SaveChanges упал
        await db.DisposeAsync();

        var handler = new CreateSmtpProfileRequestHandler(_client.Object, db);
        var result = await handler.HandleAsync(new CreateSmtpProfileRequest(
            "smtp.yandex.ru", 587, "user", "pass", "from@mail.ru", "Test"));

        Assert.False(result.success);
        _client.Verify(c => c.DeleteSmtpProfileAsync(profileId), Times.Once);
    }
}