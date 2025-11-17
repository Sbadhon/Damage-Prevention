namespace RiskSvc.Application.Common.Tenancy;

public interface ITenantScopedRequest
{
    string TenantId { get; set; }
}
