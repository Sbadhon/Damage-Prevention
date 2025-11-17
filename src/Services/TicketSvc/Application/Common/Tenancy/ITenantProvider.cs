namespace TicketSvc.Application.Common.Tenancy;

public interface ITenantProvider
{
    /// <summary>
    /// Current tenant id for this request, or null if not resolved.
    /// </summary>
    string? CurrentTenantId { get; }
}
