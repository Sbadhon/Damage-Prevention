using MediatR;
using SharedKernel;
using SchedulingSvc.Application.Common.Tenancy;
using SchedulingSvc.Application.WorkOrders.Dtos;
using SchedulingSvc.Domain.Abstractions;
using SchedulingSvc.Domain.WorkOrders;
using SchedulingSvc.Application.Crew;

namespace SchedulingSvc.Application.WorkOrders.Commands;

public sealed class CreateWorkOrderCommand : IRequest<WorkOrderDto>, ITenantScopedRequest
{
    public string TenantId { get; set; } = default!;
    public Guid TicketId { get; init; }
    public string WorkType { get; init; } = default!;
    public string Address { get; init; } = default!;
    public string? CrewId { get; private set; }
    public string? CrewName { get; private set; }
    public double Lat { get; init; }
    public double Lon { get; init; }
}

public sealed class CreateWorkOrderCommandHandler
    : IRequestHandler<CreateWorkOrderCommand, WorkOrderDto>
{
    private readonly IWorkOrderRepository _repository;
    private readonly IDateTime _clock;

    public CreateWorkOrderCommandHandler(
        IWorkOrderRepository repository,
        IDateTime clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public async Task<WorkOrderDto> Handle(CreateWorkOrderCommand request, CancellationToken ct)
    {
        var now = _clock.UtcNow;

        var workOrder = WorkOrder.CreateFromTicket(
            tenantId:  request.TenantId,
            ticketId:  request.TicketId,
            workType:  request.WorkType,
            address:   request.Address,
            lat:       request.Lat,
            lon:       request.Lon,
            createdAt: now
        );
        await _repository.AddAsync(workOrder, ct);
        await _repository.SaveChangesAsync(ct);

        return WorkOrderDto.FromEntity(workOrder);
    }
}
