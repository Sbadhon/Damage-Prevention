using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace TicketSvc.Api.Middleware;

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

        // 1) Prefer tenant from authenticated user claims (JWT)
        var user = context.User;
        if (user?.Identity is { IsAuthenticated: true })
        {
            tenantId =
                user.FindFirst("tenant")?.Value ??
                user.FindFirst("tid")?.Value ??
                user.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid")?.Value;
        }

        // 2) Fallback: header-based tenant (e.g., for local dev or non-auth calls)
        if (tenantId is null &&
            context.Request.Headers.TryGetValue(TenantHeaderName, out var values))
        {
            var raw = values.ToString().Trim();
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
