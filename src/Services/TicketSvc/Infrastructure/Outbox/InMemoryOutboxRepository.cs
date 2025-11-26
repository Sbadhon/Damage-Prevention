using System.Collections.Concurrent;
using TicketSvc.Domain.Abstractions;
using TicketSvc.Domain.Outbox;

namespace TicketSvc.Infrastructure.Outbox;

public sealed class InMemoryOutboxRepository : IOutboxRepository
{
    private readonly ConcurrentDictionary<Guid, OutboxMessage> _store = new();

    public Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        _store[message.Id] = message;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<OutboxMessage>> GetPendingBatchAsync(
        int maxItems,
        CancellationToken cancellationToken = default)
    {
        var items = _store.Values
            .Where(x => x.Status == OutboxMessageStatus.Pending)
            .OrderBy(x => x.OccurredAt)
            .Take(maxItems)
            .ToList()
            .AsReadOnly();

        return Task.FromResult((IReadOnlyList<OutboxMessage>)items);
    }

    public Task UpdateAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        // In-memory: just overwrite
        _store[message.Id] = message;
        return Task.CompletedTask;
    }
}
