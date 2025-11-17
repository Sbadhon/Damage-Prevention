using MediatR;
using TicketSvc.Application.Common.Tenancy;

namespace TicketSvc.Application.Common.Behaviors;

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
        var tenantId = _tenantProvider.CurrentTenantId;

        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new InvalidOperationException("TenantId is required for this operation.");
        }

        request.TenantId = tenantId;

        return await next();
    }
}
