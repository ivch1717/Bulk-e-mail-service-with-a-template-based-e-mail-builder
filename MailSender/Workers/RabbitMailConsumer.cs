using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MailSender;

public sealed class RabbitMailConsumer : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly RabbitOptions _rabbitOptions;
    private readonly OutboxProcessingOptions _outboxOptions;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RabbitMailConsumer> _logger;

    private IConnection? _connection;
    private readonly List<IChannel> _channels = [];
    private readonly List<IServiceScope> _scopes = [];

    public RabbitMailConsumer(
        IOptions<RabbitOptions> rabbitOptions,
        IOptions<OutboxProcessingOptions> outboxOptions,
        IOutboxRepository outboxRepository,
        IServiceScopeFactory scopeFactory,
        ILogger<RabbitMailConsumer> logger)
    {
        _rabbitOptions = rabbitOptions.Value;
        _outboxOptions = outboxOptions.Value;
        _outboxRepository = outboxRepository;
        _scopeFactory = scopeFactory;
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
                await RunConsumerLoopAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mail consumer loop failed. Retrying.");
                await Task.Delay(retryDelay, stoppingToken);
            }
            finally
            {
                await DisposeRuntimeResourcesAsync(CancellationToken.None);
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await DisposeRuntimeResourcesAsync(cancellationToken);

        await base.StopAsync(cancellationToken);
    }

    private async Task RunConsumerLoopAsync(CancellationToken cancellationToken)
    {
        var factory = RabbitConnectionFactoryProvider.Create(_rabbitOptions);
        _connection = await factory.CreateConnectionAsync(cancellationToken);

        var consumers = Math.Max(1, _rabbitOptions.Consumers);
        for (var i = 0; i < consumers; i++)
        {
            var channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
            await RabbitTopology.DeclareAsync(channel, _rabbitOptions, _outboxOptions, cancellationToken);
            await channel.BasicQosAsync(0, _rabbitOptions.Prefetch, global: false, cancellationToken);

            var scope = _scopeFactory.CreateScope();
            var smtpSender = scope.ServiceProvider.GetRequiredService<ISmtpSender>();

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += (_, ea) => HandleMessageAsync(channel, smtpSender, ea, cancellationToken);

            await channel.BasicConsumeAsync(_rabbitOptions.Queue, autoAck: false, consumer, cancellationToken);

            _channels.Add(channel);
            _scopes.Add(scope);
        }

        await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
    }

    private async Task DisposeRuntimeResourcesAsync(CancellationToken cancellationToken)
    {
        foreach (var channel in _channels)
        {
            try { await channel.CloseAsync(cancellationToken); } catch { }
            try { channel.Dispose(); } catch { }
        }
        _channels.Clear();

        foreach (var scope in _scopes)
        {
            try { scope.Dispose(); } catch { }
        }
        _scopes.Clear();

        try { _connection?.Dispose(); } catch { }
        _connection = null;
    }

    private async Task HandleMessageAsync(
        IChannel channel,
        ISmtpSender smtpSender,
        BasicDeliverEventArgs eventArgs,
        CancellationToken serviceToken)
    {
        EmailSendRequested? message = null;

        try
        {
            var json = Encoding.UTF8.GetString(eventArgs.Body.Span);
            message = JsonSerializer.Deserialize<EmailSendRequested>(json, JsonOptions)
                ?? throw new InvalidOperationException("Message body is empty.");

            if (message.MessageId == Guid.Empty)
            {
                throw new InvalidOperationException("messageId is required.");
            }

            if (string.IsNullOrWhiteSpace(message.To))
            {
                throw new InvalidOperationException("to is required.");
            }

            var deliveryLease = TimeSpan.FromSeconds(Math.Max(5, _outboxOptions.DeliveryLeaseSeconds));
            var acquisition = await _outboxRepository.TryAcquireDeliveryAsync(
                message.MessageId,
                deliveryLease,
                serviceToken);

            if (acquisition.Status is DeliveryAcquireStatus.Completed or DeliveryAcquireStatus.Busy)
            {
                await channel.BasicAckAsync(eventArgs.DeliveryTag, false, serviceToken);
                return;
            }

            try
            {
                await smtpSender.SendAsync(message, serviceToken);
                await _outboxRepository.MarkDeliveredAsync(message.MessageId, serviceToken);

                await channel.BasicAckAsync(eventArgs.DeliveryTag, false, serviceToken);
            }
            catch (Exception smtpEx)
            {
                var retryDelay = TimeSpan.FromSeconds(Math.Max(1, _outboxOptions.RetryDelaySeconds));
                var failure = await _outboxRepository.MarkDeliveryFailedAsync(
                    message.MessageId,
                    smtpEx.Message,
                    retryDelay,
                    Math.Max(1, _outboxOptions.MaxDeliveryAttempts),
                    serviceToken);

                if (failure.ShouldDeadLetter)
                {
                    await PublishToDeadLetterAsync(channel, message, failure.DeliveryAttempts, smtpEx.Message, serviceToken);
                    await channel.BasicAckAsync(eventArgs.DeliveryTag, false, serviceToken);
                    return;
                }

                _logger.LogWarning(
                    smtpEx,
                    "SMTP delivery failed for {MessageId}. Attempt {Attempt}. Moving message to retry queue.",
                    message.MessageId,
                    failure.DeliveryAttempts);

                await channel.BasicNackAsync(eventArgs.DeliveryTag, false, requeue: false, serviceToken);
            }
        }
        catch (Exception ex)
        {
            if (message is null || message.MessageId == Guid.Empty || string.IsNullOrWhiteSpace(message.To))
            {
                _logger.LogWarning(ex, "Dropping invalid mail message from queue.");
                await channel.BasicAckAsync(eventArgs.DeliveryTag, false, serviceToken);
                return;
            }

            _logger.LogWarning(ex, "Infrastructure failure while processing {MessageId}. Requeueing.", message.MessageId);
            await channel.BasicNackAsync(eventArgs.DeliveryTag, false, requeue: true, serviceToken);
        }
    }

    private async Task PublishToDeadLetterAsync(
        IChannel channel,
        EmailSendRequested message,
        int attempts,
        string error,
        CancellationToken ct)
    {
        var properties = new BasicProperties
        {
            Persistent = true,
            MessageId = message.MessageId.ToString("D"),
            ContentType = "application/json",
            Headers = new Dictionary<string, object?>
            {
                ["x-outbox-id"] = message.MessageId.ToString("D"),
                ["x-delivery-attempts"] = attempts,
                ["x-last-error"] = error
            }
        };

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message, JsonOptions));
        await channel.BasicPublishAsync(
            _rabbitOptions.DeadLetterExchange,
            _rabbitOptions.DeadLetterRoutingKey,
            mandatory: false,
            properties,
            body,
            ct);
    }
}
