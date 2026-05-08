using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using MailSender;
using System.Net.Mail;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RabbitOptions>(builder.Configuration.GetSection("Rabbit"));
builder.Services.Configure<OutboxProcessingOptions>(builder.Configuration.GetSection("Outbox"));

var connectionString = builder.Configuration.GetConnectionString("Postgres")
                       ?? throw new InvalidOperationException("Connection string 'Postgres' is not configured.");

builder.Services.AddSingleton(new PostgresOptions
{
    ConnectionString = connectionString
});
builder.Services.AddSingleton<IOutboxRepository, PostgresOutboxRepository>();
builder.Services.AddTransient<ISmtpSender, SmtpSender>();
// if (builder.Configuration.GetValue("Outbox:PublishEnabled", false))
// {
//     builder.Services.AddHostedService<RabbitOutboxPublisher>();
// }
builder.Services.AddHostedService<RabbitMailConsumer>();

var app = builder.Build();

app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));

app.MapGet("/internal/smtp", async (
    IOutboxRepository repository,
    CancellationToken ct) =>
{
    var profiles = await repository.GetSmtpProfilesAsync(ct);
    return Results.Ok(new { smtpProfiles = profiles });
});

app.MapPost("/internal/smtp", async (
    CreateSmtpProfileRequest request,
    IOutboxRepository repository,
    CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(request.Host))
    {
        return Results.BadRequest("SMTP host is required.");
    }

    if (request.Port is <= 0 or > 65535)
    {
        return Results.BadRequest("SMTP port is invalid.");
    }

    try
    {
        _ = new MailAddress(request.FromEmail);
    }
    catch
    {
        return Results.BadRequest("SMTP from email is invalid.");
    }

    var profileId = Guid.NewGuid();
    await repository.CreateSmtpProfileAsync(new SmtpProfile(
        profileId,
        request.Host.Trim(),
        request.Port,
        request.User.Trim(),
        request.Password,
        request.FromEmail.Trim(),
        request.DisplayName.Trim(),
        request.UseStartTls), ct);

    return Results.Ok(profileId);
});

app.MapDelete("/internal/smtp/{profileId:guid}", async (
    Guid profileId,
    IOutboxRepository repository,
    CancellationToken ct) =>
{
    await repository.DeleteSmtpProfileAsync(profileId, ct);
    return Results.Ok();
});

await app.RunAsync();
