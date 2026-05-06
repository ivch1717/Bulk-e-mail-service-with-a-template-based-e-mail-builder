namespace MailSender;

public interface IOutboxRepository
{
    Task InitializeAsync(CancellationToken ct);

    Task<IReadOnlyList<OutboxEmail>> AcquirePublishBatchAsync(int batchSize, TimeSpan leaseDuration, CancellationToken ct);

    Task<OutboxEmail?> GetOutboxEmailAsync(Guid messageId, CancellationToken ct);

    Task MarkPublishedAsync(Guid messageId, CancellationToken ct);

    Task ReleasePublishLeaseAsync(Guid messageId, string? error, CancellationToken ct);

    Task<OutboxDeliveryAttempt> TryAcquireDeliveryAsync(Guid messageId, TimeSpan leaseDuration, CancellationToken ct);

    Task<SmtpProfile?> GetSmtpProfileAsync(Guid profileId, CancellationToken ct);

    Task<IReadOnlyList<SmtpProfileSummary>> GetSmtpProfilesAsync(CancellationToken ct);

    Task CreateSmtpProfileAsync(SmtpProfile profile, CancellationToken ct);

    Task DeleteSmtpProfileAsync(Guid profileId, CancellationToken ct);

    Task MarkDeliveredAsync(Guid messageId, CancellationToken ct);

    Task<DeliveryFailureResult> MarkDeliveryFailedAsync(
        Guid messageId,
        string error,
        TimeSpan retryDelay,
        int maxDeliveryAttempts,
        CancellationToken ct);
}
