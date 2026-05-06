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

        var exchange = Environment.GetEnvironmentVariable("RABBIT__EXCHANGE") ?? "mail.send";
        var routingKey = Environment.GetEnvironmentVariable("RABBIT__ROUTINGKEY") ?? "send";
        await DeclareTopologyAsync(channel, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await using var transaction = await db.Database.BeginTransactionAsync(stoppingToken);

                var pending = await db.OutboxEmails.FromSqlRaw("""
                    select *
                    from "OutboxEmails"
                    where "Sent" = false
                    order by "CreatedAt", "Id"
                    for update skip locked
                    limit 50
                    """)
                    .ToListAsync(stoppingToken);

                foreach (var email in pending)
                {
                    var message = new
                    {
                        MessageId = email.Id,
                        SmtpProfileId = email.SmtpId,
                        To = email.To,
                        HtmlBody = email.Html,
                        Subject = email.Subject
                    };

                    var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
    
                    var properties = new BasicProperties
                    {
                        Persistent = true,
                        MessageId = email.Id.ToString("D"),
                        ContentType = "application/json"
                    };
    
                    await channel.BasicPublishAsync(
                        exchange: exchange,
                        routingKey: routingKey,
                        mandatory: false,
                        basicProperties: properties,
                        body: body,
                        cancellationToken: stoppingToken);

                    email.Sent = true;
                }

                if (pending.Count > 0)
                {
                    await db.SaveChangesAsync(stoppingToken);
                }

                await transaction.CommitAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Publisher error");
            }

            await Task.Delay(500, stoppingToken);
        }
    }

    private static async Task DeclareTopologyAsync(IChannel channel, CancellationToken ct)
    {
        var exchange = Environment.GetEnvironmentVariable("RABBIT__EXCHANGE") ?? "mail.send";
        var queue = Environment.GetEnvironmentVariable("RABBIT__QUEUE") ?? "mail.send.q";
        var routingKey = Environment.GetEnvironmentVariable("RABBIT__ROUTINGKEY") ?? "send";
        var retryExchange = Environment.GetEnvironmentVariable("RABBIT__RETRYEXCHANGE") ?? "mail.send.retry";
        var retryRoutingKey = Environment.GetEnvironmentVariable("RABBIT__RETRYROUTINGKEY") ?? "send.retry";

        await channel.ExchangeDeclareAsync(
            exchange,
            ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: ct);

        await channel.QueueDeclareAsync(
            queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                ["x-dead-letter-exchange"] = retryExchange,
                ["x-dead-letter-routing-key"] = retryRoutingKey
            },
            cancellationToken: ct);

        await channel.QueueBindAsync(
            queue,
            exchange,
            routingKey,
            cancellationToken: ct);
    }
}
