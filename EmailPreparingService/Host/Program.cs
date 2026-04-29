using Infrastructure;
using Microsoft.EntityFrameworkCore;
using UseCases;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddUseCases();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.MapControllers(); 
app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
} // TODO: убрать сваггер после написания фронта
app.Run();
