using MailSender;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<RabbitOptions>(builder.Configuration.GetSection("Rabbit"));

builder.Services.AddSingleton<IIdempotencyStore, FileIdempotencyStore>();
builder.Services.AddTransient<ISmtpSender, SmtpSender>();
builder.Services.AddHostedService<RabbitWorker>();

await builder.Build().RunAsync();