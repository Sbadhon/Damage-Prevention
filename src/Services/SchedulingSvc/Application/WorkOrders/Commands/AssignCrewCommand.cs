using MediatR;
using SharedKernel;
using SchedulingSvc.Application.Common.Tenancy;
using SchedulingSvc.Domain.Abstractions;
using MassTransit;
using Contracts.Scheduling;

namespace SchedulingSvc.Application.WorkOrders.Commands;

public sealed class AssignCrewCommand : IRequest<Unit>, ITenantScopedRequest
{
    public string TenantId { get; set; } = default!;
    public Guid WorkOrderId { get; init; }
    public string CrewId { get; init; } = default!;
    public string CrewName { get; init; } = default!;
}

public sealed class AssignCrewCommandHandler
    : IRequestHandler<AssignCrewCommand, Unit>
{
    private readonly IWorkOrderRepository _repository;
    private readonly IDateTime _clock;
    private readonly IPublishEndpoint _publisher;

    public AssignCrewCommandHandler(IWorkOrderRepository repository, IDateTime clock, IPublishEndpoint publisher)
    {
        _repository = repository;
        _clock = clock;
        _publisher = publisher;
    }

    public async Task<Unit> Handle(AssignCrewCommand request, CancellationToken ct)
    {
        var wo = await _repository.GetByIdAsync(request.WorkOrderId, ct);
        if (wo is null)
            throw new InvalidOperationException($"WorkOrder {request.WorkOrderId} not found.");

        if (!string.Equals(wo.TenantId, request.TenantId, StringComparison.Ordinal))
            throw new InvalidOperationException("Work order does not belong to current tenant.");

        wo.AssignCrew(request.CrewId, request.CrewName, _clock.UtcNow);

        await _repository.SaveChangesAsync(ct);
        
        await _publisher.Publish(new WorkOrderAssignedEvent(
            wo.Id,
            wo.TicketId,
            wo.CrewId!,
            wo.CrewName!,
            wo.AssignedAt!.Value
        ), ct);

        return Unit.Value;
    }
}
