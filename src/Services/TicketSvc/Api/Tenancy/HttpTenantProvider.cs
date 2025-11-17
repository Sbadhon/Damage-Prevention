using Microsoft.AspNetCore.Http;
using TicketSvc.Api.Middleware;
using TicketSvc.Application.Common.Tenancy;

namespace TicketSvc.Api.Tenancy;

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
