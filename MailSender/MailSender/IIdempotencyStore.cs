namespace MailSender;

public interface IIdempotencyStore
{
    Task InitAsync(CancellationToken ct);

    bool TryBegin(Guid messageId);

    Task MarkSuccessAsync(Guid messageId, CancellationToken ct);

    void MarkFailed(Guid messageId);
}