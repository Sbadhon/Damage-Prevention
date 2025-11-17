using Microsoft.AspNetCore.Http;
using RiskSvc.Api.Middleware;
using RiskSvc.Application.Common.Tenancy;

namespace RiskSvc.Api.Tenancy;

public sealed class HttpTenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpTenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? CurrentTenantId
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext is null)
                return null;

            if (httpContext.Items.TryGetValue(TenantResolutionMiddleware.TenantItemKey, out var value)
                && value is string tenantId)
            {
                return tenantId;
            }

            return null;
        }
    }
}
