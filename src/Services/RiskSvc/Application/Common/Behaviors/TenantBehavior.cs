using MediatR;
using RiskSvc.Application.Common.Tenancy;

namespace RiskSvc.Application.Common.Behaviors;

public sealed class TenantBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ITenantScopedRequest, IRequest<TResponse>
{
    private readonly ITenantProvider _tenantProvider;

    public TenantBehavior(ITenantProvider tenantProvider)
    {
        _tenantProvider = tenantProvider;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }
        
        if (!string.IsNullOrWhiteSpace(request.TenantId))
        {
            return await next();
        }

        // 2️⃣ Otherwise, we're in an HTTP context and expect X-Tenant-Id header
        var ambientTenantId = _tenantProvider.CurrentTenantId;

        if (string.IsNullOrWhiteSpace(ambientTenantId))
        {
            throw new InvalidOperationException("TenantId is required for this operation.");
        }

        request.TenantId = ambientTenantId;

        return await next();
    }
}
