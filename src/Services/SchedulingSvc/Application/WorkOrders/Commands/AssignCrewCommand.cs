using MediatR;
using SharedKernel;
using SchedulingSvc.Application.Common.Tenancy;
using SchedulingSvc.Domain.Abstractions;

namespace SchedulingSvc.Application.WorkOrders.Commands;

public sealed class AssignCrewCommand : IRequest<Unit>, ITenantScopedRequest
{
    public string TenantId { get; set; } = default!;
    public Guid WorkOrderId { get; init; }
    public string CrewId { get; init; } = default!;
}

public sealed class AssignCrewCommandHandler
    : IRequestHandler<AssignCrewCommand, Unit>
{
    private readonly IWorkOrderRepository _repository;
    private readonly IDateTime _clock;

    public AssignCrewCommandHandler(IWorkOrderRepository repository, IDateTime clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public async Task<Unit> Handle(AssignCrewCommand request, CancellationToken ct)
    {
        var wo = await _repository.GetByIdAsync(request.WorkOrderId, ct);
        if (wo is null)
            throw new InvalidOperationException($"WorkOrder {request.WorkOrderId} not found.");

        if (!string.Equals(wo.TenantId, request.TenantId, StringComparison.Ordinal))
            throw new InvalidOperationException("Work order does not belong to current tenant.");

        wo.AssignCrew(request.CrewId, _clock.UtcNow);

        await _repository.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
