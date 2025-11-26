using System.Collections.Concurrent;
using TicketSvc.Domain.Abstractions;
using TicketSvc.Domain.Tickets;

namespace TicketSvc.Infrastructure.Tickets;

public sealed class InMemoryTicketRepository : ITicketRepository
{
    private readonly ConcurrentDictionary<Guid, Ticket> _store = new();

    public Task<Ticket?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(id, out var ticket);
        return Task.FromResult(ticket);
    }

    public Task AddAsync(
        Ticket ticket,
        CancellationToken cancellationToken = default)
    {
        _store[ticket.Id] = ticket;
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // In-memory store has no real persistence.
        return Task.CompletedTask;
    }

    public Task<(IReadOnlyList<Ticket> Items, int TotalCount)> ListByTenantAsync(
        string tenantId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _store.Values
            .Where(t => string.Equals(t.TenantId.Value, tenantId, StringComparison.Ordinal))
            .OrderByDescending(t => t.CreatedAt);

        var total = query.Count();

        var items = query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult(((IReadOnlyList<Ticket>)items, total));
    }
}
