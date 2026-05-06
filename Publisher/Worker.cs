using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

namespace Publisher;

public class Worker(IServiceScopeFactory scopeFactory, ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = Environment.GetEnvironmentVariable("RABBIT__HOST") ?? "rabbitmq",
            UserName = Environment.GetEnvironmentVariable("RABBIT__USER") ?? "guest",
            Password = Environment.GetEnvironmentVariable("RABBIT__PASS") ?? "guest"
        };

        await using var connection = await factory.CreateConnectionAsync(stoppingToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var pending = await db.OutboxEmails
                    .Where(e => !e.Sent)
                    .Take(50)
                    .ToListAsync(stoppingToken);

                foreach (var email in pending)
                {
                    var message = new
                    {
                        MessageId = email.Id,
                        To = email.To,
                        HtmlBody = email.Html,
                        Subject = email.Subject,
                        FromEmail = (string?)null,
                        FromName = (string?)null
                    };

                    var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
    
                    var properties = new BasicProperties
                    {
                        Persistent = true,
                        MessageId = email.Id.ToString("D"),
                        ContentType = "application/json"
                    };
    
                    await channel.BasicPublishAsync(
                        exchange: "mail.send",
                        routingKey: "send",
                        mandatory: false,
                        basicProperties: properties,
                        body: body,
                        cancellationToken: stoppingToken);

                    email.Sent = true;
                }

                if (pending.Count > 0)
                    await db.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Publisher error");
            }

            await Task.Delay(500, stoppingToken);
        }
    }
}