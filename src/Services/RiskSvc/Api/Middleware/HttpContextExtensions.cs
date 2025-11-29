using Microsoft.AspNetCore.Http;
using RiskSvc.Api.Middleware;
using System;

namespace RiskSvc.Api.Middleware
{
    public static class HttpContextExtensions
    {
        public static string GetTenantId(this HttpContext context)
        {
            if (context.Items.TryGetValue(TenantResolutionMiddleware.TenantItemKey, out var value)
                && value is string tenantId
                && !string.IsNullOrEmpty(tenantId))
            {
                return tenantId;
            }

            throw new InvalidOperationException("Tenant ID is not set in the current context.");
        }
    }
}
