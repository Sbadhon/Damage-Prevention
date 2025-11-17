namespace TicketSvc.Application.Common.Tenancy;

public interface ITenantScopedRequest
{
    /// <summary>
    /// Tenant id that this request is executing under.
    /// </summary>
    string TenantId { get; set; }
}
