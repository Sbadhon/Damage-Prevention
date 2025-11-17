using MediatR;
using SchedulingSvc.Application.Common.Tenancy;
using SchedulingSvc.Application.WorkOrders.Dtos;
using SchedulingSvc.Domain.Abstractions;

namespace SchedulingSvc.Application.WorkOrders.Queries;

public sealed class GetWorkOrderByIdQuery
    : IRequest<WorkOrderDto?>, ITenantScopedRequest
{
    public string TenantId { get; set; } = default!;
    public Guid WorkOrderId { get; init; }
}

public sealed class GetWorkOrderByIdQueryHandler
    : IRequestHandler<GetWorkOrderByIdQuery, WorkOrderDto?>
{
    private readonly IWorkOrderRepository _repository;

    public GetWorkOrderByIdQueryHandler(IWorkOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<WorkOrderDto?> Handle(
        GetWorkOrderByIdQuery request,
        CancellationToken ct)
    {
        var wo = await _repository.GetByIdAsync(request.WorkOrderId, ct);
        if (wo is null)
            return null;

        if (!string.Equals(wo.TenantId, request.TenantId, StringComparison.Ordinal))
            return null;

        return WorkOrderDto.FromEntity(wo);
    }
}
