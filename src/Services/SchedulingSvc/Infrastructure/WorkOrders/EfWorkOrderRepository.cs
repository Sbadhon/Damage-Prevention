using Microsoft.EntityFrameworkCore;
using SchedulingSvc.Domain.Abstractions;
using SchedulingSvc.Domain.WorkOrders;

namespace SchedulingSvc.Infrastructure.WorkOrders;

public sealed class EfWorkOrderRepository : IWorkOrderRepository
{
    private readonly SchedulingDbContext _db;

    public EfWorkOrderRepository(SchedulingDbContext db)
    {
        _db = db;
    }

    public async Task<WorkOrder?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.WorkOrders
            //.AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == id, ct);
    }

    public async Task AddAsync(WorkOrder workOrder, CancellationToken ct = default)
    {
        await _db.WorkOrders.AddAsync(workOrder, ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        return _db.SaveChangesAsync(ct);
    }

    public async Task<(IReadOnlyList<WorkOrder> Items, int TotalCount)> ListByTenantAsync(
        string tenantId,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = _db.WorkOrders
            .AsNoTracking()
            .Where(w => w.TenantId == tenantId)
            .OrderByDescending(w => w.CreatedAt);

        var total = await query.CountAsync(ct);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }
}
