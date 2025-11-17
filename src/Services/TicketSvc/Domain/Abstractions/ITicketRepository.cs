using TicketSvc.Domain.Tickets;

namespace TicketSvc.Domain.Abstractions;

public interface ITicketRepository
{
    Task<Ticket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Ticket ticket, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a page of tickets for the given tenant, ordered by CreatedAt descending.
    /// </summary>
    Task<(IReadOnlyList<Ticket> Items, int TotalCount)> ListByTenantAsync(
        string tenantId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}
