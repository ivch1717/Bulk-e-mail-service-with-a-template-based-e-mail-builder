using MailSender;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<RabbitOptions>(builder.Configuration.GetSection("Rabbit"));
builder.Services.Configure<OutboxProcessingOptions>(builder.Configuration.GetSection("Outbox"));

builder.Services.AddSingleton(new PostgresOptions
{
    ConnectionString =
        builder.Configuration.GetConnectionString("Postgres")
        ?? builder.Configuration["Postgres:ConnectionString"]
        ?? "Host=localhost;Port=5432;Database=emailservice;Username=postgres;Password=postgres"
});

builder.Services.AddSingleton<IOutboxRepository, PostgresOutboxRepository>();
builder.Services.AddTransient<ISmtpSender, SmtpSender>();
builder.Services.AddHostedService<RabbitOutboxPublisher>();
builder.Services.AddHostedService<RabbitMailConsumer>();

await builder.Build().RunAsync();
