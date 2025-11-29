using Microsoft.EntityFrameworkCore;
using RiskSvc.Domain.Abstractions;
using RiskSvc.Domain.Risk;

namespace RiskSvc.Infrastructure.Risk;

public sealed class EfCoreRiskAssessmentRepository : IRiskAssessmentRepository
{
    private readonly RiskDbContext _db;

    public EfCoreRiskAssessmentRepository(RiskDbContext db)
    {
        _db = db;
    }

    public async Task<RiskAssessment?> GetByTicketIdAsync(Guid ticketId, CancellationToken ct = default)
    {
        return await _db.RiskAssessments
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.TicketId == ticketId, ct);
    }

    public async Task AddAsync(RiskAssessment risk, CancellationToken ct = default)
    {
        await _db.RiskAssessments.AddAsync(risk, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _db.SaveChangesAsync(ct);
    }

    public async Task<(IReadOnlyList<RiskAssessment> Items, int TotalCount)> ListByTenantAsync(
        string tenantId,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 20;

        var query = _db.RiskAssessments
            .AsNoTracking()
            .Where(r => r.TenantId == tenantId)
            .OrderByDescending(r => r.AssessedAt);

        var total = await query.CountAsync(ct);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }
}
