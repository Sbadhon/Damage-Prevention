using System.Collections.Concurrent;
using RiskSvc.Domain.Abstractions;
using RiskSvc.Domain.Risk;

namespace RiskSvc.Infrastructure.Risk;

public sealed class InMemoryRiskAssessmentRepository : IRiskAssessmentRepository
{
    private readonly ConcurrentDictionary<Guid, RiskAssessment> _store = new();

    public Task<RiskAssessment?> GetByTicketIdAsync(Guid ticketId, CancellationToken ct = default)
    {
        var match = _store.Values.FirstOrDefault(r => r.TicketId == ticketId);
        return Task.FromResult(match);
    }

    public Task AddAsync(RiskAssessment risk, CancellationToken ct = default)
    {
        _store[risk.Id] = risk;
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        // In-memory: nothing to flush.
        return Task.CompletedTask;
    }

    public Task<(IReadOnlyList<RiskAssessment> Items, int TotalCount)> ListByTenantAsync(
        string tenantId,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize   < 1) pageSize   = 20;

        var query = _store.Values
            .Where(r => string.Equals(r.TenantId, tenantId, StringComparison.Ordinal))
            .OrderByDescending(r => r.AssessedAt);

        var total = query.Count();
        var items = query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult(((IReadOnlyList<RiskAssessment>)items, total));
    }
}
