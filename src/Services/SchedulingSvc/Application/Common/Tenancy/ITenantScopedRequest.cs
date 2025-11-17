namespace SchedulingSvc.Application.Common.Tenancy;

public interface ITenantScopedRequest
{
    string TenantId { get; set; }
}
