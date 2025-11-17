using Microsoft.AspNetCore.Http;
using SchedulingSvc.Api.Middleware;
using SchedulingSvc.Application.Common.Tenancy;

namespace SchedulingSvc.Api.Tenancy;

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
