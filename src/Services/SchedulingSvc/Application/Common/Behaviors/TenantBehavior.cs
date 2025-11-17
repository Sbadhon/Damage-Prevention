using MediatR;
using SchedulingSvc.Application.Common.Tenancy;

namespace SchedulingSvc.Application.Common.Behaviors;

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
        // If TenantId is already set (e.g. from a background processor or system command),
        // do NOT override it and do NOT require HttpContext.
        if (!string.IsNullOrWhiteSpace(request.TenantId))
        {
            return await next();
        }

        var tenantId = _tenantProvider.CurrentTenantId;

        if (string.IsNullOrWhiteSpace(tenantId))
            throw new InvalidOperationException("TenantId is required for this operation.");

        request.TenantId = tenantId;

        return await next();
    }
}
