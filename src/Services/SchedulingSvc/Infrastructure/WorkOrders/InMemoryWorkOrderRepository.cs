using System.Collections.Concurrent;
using SchedulingSvc.Domain.Abstractions;
using SchedulingSvc.Domain.WorkOrders;

namespace SchedulingSvc.Infrastructure.WorkOrders;

public sealed class InMemoryWorkOrderRepository : IWorkOrderRepository
{
    private readonly ConcurrentDictionary<Guid, WorkOrder> _store = new();

    public Task<WorkOrder?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        _store.TryGetValue(id, out var wo);
        return Task.FromResult(wo);
    }

    public Task AddAsync(WorkOrder workOrder, CancellationToken ct = default)
    {
        _store[workOrder.Id] = workOrder;
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        // In-memory store: nothing to do.
        return Task.CompletedTask;
    }

    public Task<(IReadOnlyList<WorkOrder> Items, int TotalCount)> ListByTenantAsync(
        string tenantId,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 20;

        var query = _store.Values
            .Where(w => string.Equals(w.TenantId, tenantId, StringComparison.Ordinal))
            .OrderByDescending(w => w.CreatedAt);

        var total = query.Count();
        var items = query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult(((IReadOnlyList<WorkOrder>)items, total));
    }
}
