using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MailSender;

public sealed class RabbitWorker : BackgroundService
{
    private readonly RabbitOptions _opt;
    private readonly IIdempotencyStore _idem;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RabbitWorker> _log;

    private IConnection? _conn;

    private readonly List<IChannel> _channels = new();
    private readonly List<IServiceScope> _scopes = new();

    private static readonly JsonSerializerOptions JsonOpt = new() { PropertyNameCaseInsensitive = true };

    public RabbitWorker(
        IOptions<RabbitOptions> opt,
        IIdempotencyStore idem,
        IServiceScopeFactory scopeFactory,
        ILogger<RabbitWorker> log)
    {
        _opt = opt.Value;
        _idem = idem;
        _scopeFactory = scopeFactory;
        _log = log;
    }

    public override async Task StartAsync(CancellationToken ct)
    {
        await _idem.InitAsync(ct);

        var factory = new ConnectionFactory
        {
            HostName = _opt.Host,
            Port = _opt.Port,
            UserName = _opt.User,
            Password = _opt.Pass,
        };

        _conn = await factory.CreateConnectionAsync(ct);

        var consumers = Math.Max(1, _opt.Consumers);
        for (int i = 0; i < consumers; i++)
        {
            var ch = await _conn.CreateChannelAsync(cancellationToken: ct);

            await ch.ExchangeDeclareAsync(_opt.Exchange, ExchangeType.Direct, durable: true, autoDelete: false, cancellationToken: ct);
            await ch.QueueDeclareAsync(_opt.Queue, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: ct);
            await ch.QueueBindAsync(_opt.Queue, _opt.Exchange, _opt.RoutingKey, cancellationToken: ct);

            await ch.BasicQosAsync(0, _opt.Prefetch, global: false, ct);

            var scope = _scopeFactory.CreateScope();
            _scopes.Add(scope);

            var smtp = scope.ServiceProvider.GetRequiredService<ISmtpSender>();

            var consumer = new AsyncEventingBasicConsumer(ch);
            consumer.ReceivedAsync += async (_, ea) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.Span);
                    var msg = JsonSerializer.Deserialize<EmailSendRequested>(json, JsonOpt)
                              ?? throw new InvalidOperationException("Bad JSON");

                    if (msg.MessageId == Guid.Empty) throw new InvalidOperationException("messageId required");
                    if (string.IsNullOrWhiteSpace(msg.To)) throw new InvalidOperationException("to required");

                    if (!_idem.TryBegin(msg.MessageId))
                    {
                        await ch.BasicAckAsync(ea.DeliveryTag, false, ct);
                        return;
                    }

                    await smtp.SendAsync(msg, ct);
                    await _idem.MarkSuccessAsync(msg.MessageId, ct);

                    await ch.BasicAckAsync(ea.DeliveryTag, false, ct);
                }
                catch (Exception ex)
                {
                    try
                    {
                        var json = Encoding.UTF8.GetString(ea.Body.Span);
                        var tmp = JsonSerializer.Deserialize<EmailSendRequested>(json, JsonOpt);
                        if (tmp?.MessageId != Guid.Empty) _idem.MarkFailed(tmp.MessageId);
                    }
                    catch
                    {
                        // ignored
                    }

                    _log.LogWarning(ex, "Failed. Requeue");
                    await ch.BasicNackAsync(ea.DeliveryTag, false, requeue: true, ct);
                }
            };

            await ch.BasicConsumeAsync(_opt.Queue, autoAck: false, consumer, ct);

            _channels.Add(ch);
        }

        await base.StartAsync(ct);
    }

    protected override Task ExecuteAsync(CancellationToken ct)
        => Task.Delay(Timeout.InfiniteTimeSpan, ct);

    public override async Task StopAsync(CancellationToken ct)
    {
        foreach (var ch in _channels)
        {
            try { await ch.CloseAsync(ct); } catch { }
            try { ch.Dispose(); } catch { }
        }
        _channels.Clear();

        foreach (var scope in _scopes)
        {
            try { scope.Dispose(); } catch { }
        }
        _scopes.Clear();

        try { _conn?.Dispose(); } catch { }
        _conn = null;

        await base.StopAsync(ct);
    }
}
