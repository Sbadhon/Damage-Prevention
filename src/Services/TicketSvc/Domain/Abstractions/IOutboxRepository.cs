using TicketSvc.Domain.Outbox;

namespace TicketSvc.Domain.Abstractions;

public interface IOutboxRepository
{
    Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a batch of messages that are still pending (or failed, if you want retries later).
    /// For now we'll just pull Pending only.
    /// </summary>
    Task<IReadOnlyList<OutboxMessage>> GetPendingBatchAsync(
        int maxItems,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(OutboxMessage message, CancellationToken cancellationToken = default);
}
