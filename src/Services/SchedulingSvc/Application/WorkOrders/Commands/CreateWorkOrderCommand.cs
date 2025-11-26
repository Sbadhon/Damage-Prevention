using MediatR;
using SharedKernel;
using SchedulingSvc.Application.Common.Tenancy;
using SchedulingSvc.Domain.Abstractions;
using SchedulingSvc.Domain.WorkOrders;

namespace SchedulingSvc.Application.WorkOrders.Commands;

public sealed class CreateWorkOrderCommand : IRequest<Guid>, ITenantScopedRequest
{
    public string TenantId { get; set; } = default!;
    public Guid TicketId { get; init; }
    public string WorkType { get; init; } = default!;
    public string Address { get; init; } = default!;
    public double Lat { get; init; }
    public double Lon { get; init; }
}

public sealed class CreateWorkOrderCommandHandler
    : IRequestHandler<CreateWorkOrderCommand, Guid>
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

    public async Task<Guid> Handle(CreateWorkOrderCommand request, CancellationToken ct)
    {
        var now = _clock.UtcNow;

        var wo = WorkOrder.CreateFromTicket(
            request.TenantId,
            request.TicketId,
            request.WorkType,
            request.Address,
            request.Lat,
            request.Lon,
            now);

        await _repository.AddAsync(wo, ct);
        await _repository.SaveChangesAsync(ct);

        return wo.Id;
    }
}
