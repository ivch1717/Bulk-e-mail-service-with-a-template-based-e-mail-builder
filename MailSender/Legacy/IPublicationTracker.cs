namespace MailSender;

public interface IPublicationTracker
{
    Task<bool> TryReserveAsync(Guid messageId, DateTimeOffset now, TimeSpan staleAfter, CancellationToken ct);

    Task ReleaseAsync(Guid messageId, CancellationToken ct);

    Task CompleteAsync(Guid messageId, CancellationToken ct);
}
