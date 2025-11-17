namespace RiskSvc.Application.Common.Tenancy;

public interface ITenantProvider
{
    string? CurrentTenantId { get; }
}
