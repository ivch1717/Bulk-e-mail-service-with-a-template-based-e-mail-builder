using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Npgsql;
using RabbitMQ.Client;

namespace MailSender;

public sealed class RabbitOutboxPublisher : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RabbitOptions _rabbitOptions;
    private readonly OutboxProcessingOptions _outboxOptions;
    private readonly IOutboxRepository _outboxRepository;
    private readonly ILogger<RabbitOutboxPublisher> _logger;

    public RabbitOutboxPublisher(
        IOptions<RabbitOptions> rabbitOptions,
        IOptions<OutboxProcessingOptions> outboxOptions,
        IOutboxRepository outboxRepository,
        ILogger<RabbitOutboxPublisher> logger)
    {
        _rabbitOptions = rabbitOptions.Value;
        _outboxOptions = outboxOptions.Value;
        _outboxRepository = outboxRepository;
        _logger = logger;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await _outboxRepository.InitializeAsync(cancellationToken);
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var retryDelay = TimeSpan.FromSeconds(5);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunPublisherLoopAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (PostgresException ex) when (ex.SqlState == "42P01")
            {
                _logger.LogInformation(
                    "Outbox table is not ready yet. Waiting for database migration before publishing starts.");
                await Task.Delay(retryDelay, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox publisher loop failed. Retrying.");
                await Task.Delay(retryDelay, stoppingToken);
            }
        }
    }

    private async Task RunPublisherLoopAsync(CancellationToken ct)
    {
        var pollInterval = TimeSpan.FromMilliseconds(Math.Max(100, _outboxOptions.PollIntervalMilliseconds));
        var publishLease = TimeSpan.FromSeconds(Math.Max(5, _outboxOptions.PublishLeaseSeconds));

        var factory = RabbitConnectionFactoryProvider.Create(_rabbitOptions);
        await using var connection = await factory.CreateConnectionAsync(ct);
        await using var channel = await connection.CreateChannelAsync(
            new CreateChannelOptions(true, true, null, 1),
            ct);

        await RabbitTopology.DeclareAsync(channel, _rabbitOptions, _outboxOptions, ct);

        while (!ct.IsCancellationRequested)
        {
            var pending = await _outboxRepository.AcquirePublishBatchAsync(
                _outboxOptions.BatchSize,
                publishLease,
                ct);

            foreach (var email in pending)
            {
                try
                {
                    var message = new EmailSendRequested(
                        email.Id,
                        email.To,
                        email.Html,
                        Subject: null,
                        FromEmail: null,
                        FromName: null);

                    var properties = new BasicProperties
                    {
                        Persistent = true,
                        MessageId = email.Id.ToString("D"),
                        ContentType = "application/json"
                    };

                    var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message, JsonOptions));

                    await channel.BasicPublishAsync(
                        _rabbitOptions.Exchange,
                        _rabbitOptions.RoutingKey,
                        mandatory: false,
                        properties,
                        body,
                        ct);

                    await _outboxRepository.MarkPublishedAsync(email.Id, ct);
                }
                catch (Exception ex)
                {
                    await _outboxRepository.ReleasePublishLeaseAsync(email.Id, ex.Message, ct);
                    throw;
                }
            }

            await Task.Delay(pollInterval, ct);
        }
    }
}
