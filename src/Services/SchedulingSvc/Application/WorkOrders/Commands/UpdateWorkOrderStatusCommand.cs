using MediatR;
using SharedKernel;
using SchedulingSvc.Application.Common.Tenancy;
using SchedulingSvc.Domain.Abstractions;
using SchedulingSvc.Domain.WorkOrders;
using MassTransit;
using Contracts.Scheduling;

namespace SchedulingSvc.Application.WorkOrders.Commands;

public sealed class UpdateWorkOrderStatusCommand : IRequest<Unit>, ITenantScopedRequest
{
    public string TenantId { get; set; } = default!;
    public Guid WorkOrderId { get; init; }
    public WorkOrderStatus Status { get; init; }
}

public sealed class UpdateWorkOrderStatusCommandHandler
    : IRequestHandler<UpdateWorkOrderStatusCommand, Unit>
{
    private readonly IWorkOrderRepository _repository;
    private readonly IDateTime _clock;
    private readonly IPublishEndpoint _publisher;

    public UpdateWorkOrderStatusCommandHandler(
        IWorkOrderRepository repository,
        IDateTime clock,
        IPublishEndpoint publisher)
    {
        _repository = repository;
        _clock = clock;
        _publisher = publisher;
    }

    public async Task<Unit> Handle(
        UpdateWorkOrderStatusCommand request,
        CancellationToken ct)
    {
        var wo = await _repository.GetByIdAsync(request.WorkOrderId, ct);

        if (wo is null)
            throw new InvalidOperationException($"WorkOrder {request.WorkOrderId} not found.");

        if (!string.Equals(wo.TenantId, request.TenantId, StringComparison.Ordinal))
            throw new InvalidOperationException("Work order does not belong to current tenant.");

        switch (request.Status)
        {
            case WorkOrderStatus.Completed:
                wo.Complete(_clock.UtcNow);
                await _publisher.Publish(new WorkOrderCompletedEvent
                {
                    WorkOrderId = wo.Id,
                    TicketId = wo.TicketId,
                    CompletedAt = _clock.UtcNow
                }, ct);
                break;

            case WorkOrderStatus.Cancelled:
                wo.Cancel(_clock.UtcNow);
                await _publisher.Publish(new WorkOrderCancelledEvent
                {
                    WorkOrderId = wo.Id,
                    TicketId = wo.TicketId,
                    CancelledAt = _clock.UtcNow
                }, ct);
                break;

            default:
                // No action for other statuses
                break;
        }

        await _repository.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
