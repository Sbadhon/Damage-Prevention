using SchedulingSvc.Domain.WorkOrders;

namespace SchedulingSvc.Domain.Abstractions;

public interface IWorkOrderRepository
{
    Task<WorkOrder?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(WorkOrder workOrder, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);

    Task<(IReadOnlyList<WorkOrder> Items, int TotalCount)> ListByTenantAsync(
        string tenantId,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default);
}
