using Microsoft.EntityFrameworkCore;
using TicketSvc.Domain.Abstractions;
using TicketSvc.Domain.Tickets;
using SharedKernel.Tenancy;  // <-- IMPORTANT: for TenantId

namespace TicketSvc.Infrastructure.Tickets;

public sealed class EfTicketRepository : ITicketRepository
{
    private readonly TicketDbContext _db;

    public EfTicketRepository(TicketDbContext db)
    {
        _db = db;
    }

    public async Task<Ticket?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Tickets
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }


    public async Task AddAsync(Ticket ticket, CancellationToken ct = default)
    {
        await _db.Tickets.AddAsync(ticket, ct);
        await _db.SaveChangesAsync(ct); // persist immediately
    }


    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);

    public async Task<(IReadOnlyList<Ticket> Items, int TotalCount)> ListByTenantAsync(
        string tenantId,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        // Convert incoming string -> TenantId value object
        var tenantIdVo = TenantId.From(tenantId);

        var baseQuery = _db.Tickets
            .AsNoTracking()
            .Where(t => t.TenantId == tenantIdVo);

        var total = await baseQuery.CountAsync(ct);

        var items = await baseQuery
            .OrderByDescending(t => t.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }
}
