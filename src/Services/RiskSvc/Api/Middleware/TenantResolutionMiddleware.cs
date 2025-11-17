using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace RiskSvc.Api.Middleware;

public sealed class TenantResolutionMiddleware
{
    public const string TenantItemKey    = "__tenantId";
    public const string TenantHeaderName = "X-Tenant-Id";

    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        string? tenantId = null;

        if (context.Request.Headers.TryGetValue(TenantHeaderName, out var headerValue))
        {
            var raw = headerValue.FirstOrDefault();
            if (!string.IsNullOrEmpty(raw))
            {
                tenantId = raw;
            }
        }

        if (tenantId is not null)
        {
            context.Items[TenantItemKey] = tenantId;
        }

        await _next(context);
    }
}
