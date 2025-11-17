using RiskSvc.Domain.Risk;

namespace RiskSvc.Domain.Abstractions;

public interface IRiskAssessmentRepository
{
    Task<RiskAssessment?> GetByTicketIdAsync(Guid ticketId, CancellationToken ct = default);
    Task AddAsync(RiskAssessment risk, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);

    Task<(IReadOnlyList<RiskAssessment> Items, int TotalCount)> ListByTenantAsync(
        string tenantId,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default);
}
