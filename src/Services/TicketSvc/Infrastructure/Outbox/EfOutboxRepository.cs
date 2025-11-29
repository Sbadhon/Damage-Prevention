using TicketSvc.Domain.Abstractions;
using TicketSvc.Domain.Outbox;
using TicketSvc.Infrastructure.Tickets;

public class EfOutboxRepository : IOutboxRepository
{
    private readonly TicketDbContext _db;

    public EfOutboxRepository(TicketDbContext db) => _db = db;

    public Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        _db.OutboxMessages.Add(message);
        return _db.SaveChangesAsync(cancellationToken);
    }

    public Task UpdateAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        _db.OutboxMessages.Update(message);
        return _db.SaveChangesAsync(cancellationToken);
    }

    public Task<IReadOnlyList<OutboxMessage>> GetPendingBatchAsync(int maxItems, CancellationToken cancellationToken = default)
    {
        var items = _db.OutboxMessages
            .Where(m => m.Status == OutboxMessageStatus.Pending)
            .OrderBy(m => m.OccurredAt)
            .Take(maxItems)
            .ToList()
            .AsReadOnly();

        return Task.FromResult((IReadOnlyList<OutboxMessage>)items);
    }
}
