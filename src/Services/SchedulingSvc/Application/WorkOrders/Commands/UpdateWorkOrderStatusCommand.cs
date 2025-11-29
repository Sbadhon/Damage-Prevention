using MediatR;
using SharedKernel;
using SchedulingSvc.Application.Common.Tenancy;
using SchedulingSvc.Domain.Abstractions;
using SchedulingSvc.Domain.WorkOrders;

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

    public UpdateWorkOrderStatusCommandHandler(
        IWorkOrderRepository repository,
        IDateTime clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public async Task<Unit> Handle(
        UpdateWorkOrderStatusCommand request,
        CancellationToken ct)
    {
                Console.WriteLine("gets here" + request.Status );
         Console.WriteLine( request.WorkOrderId);
        var wo = await _repository.GetByIdAsync(request.WorkOrderId, ct);

        if (wo is null)
            throw new InvalidOperationException($"WorkOrder {request.WorkOrderId} not found.");

        if (!string.Equals(wo.TenantId, request.TenantId, StringComparison.Ordinal))
            throw new InvalidOperationException("Work order does not belong to current tenant.");
       switch (request.Status)
        {
            case WorkOrderStatus.Completed:
                wo.Complete(_clock.UtcNow);
                break;

            case WorkOrderStatus.Cancelled:
                wo.Cancel(_clock.UtcNow);
                break;
            default:
                break;
        }

        await _repository.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
