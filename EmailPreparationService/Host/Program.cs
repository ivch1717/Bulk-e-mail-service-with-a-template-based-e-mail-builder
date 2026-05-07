using Infrastructure;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using UseCases;
using UseCases.MailSender;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddUseCases();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient<IMailSenderClient, MailSenderClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["MailSender:BaseAddress"] ?? "http://mail-sender");
});


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.UseExceptionHandler(err => err.Run(async context =>
{
    var feature = context.Features.Get<IExceptionHandlerFeature>();
    var ex = feature?.Error;

    context.Response.ContentType = "application/json";
    context.Response.StatusCode = ex switch
    {
        ArgumentException => 400,
        _ => 500
    };

    await context.Response.WriteAsJsonAsync(new { error = ex?.Message ?? "Unknown error" });
}));

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
} // TODO: убрать сваггер после написания фронта
app.Run();
