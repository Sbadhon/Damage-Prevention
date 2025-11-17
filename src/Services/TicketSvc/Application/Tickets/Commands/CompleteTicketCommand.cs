using MediatR;
using SharedKernel;
using TicketSvc.Application.Common.Tenancy;
using TicketSvc.Domain.Abstractions;

namespace TicketSvc.Application.Tickets.Commands;

public sealed class CompleteTicketCommand : IRequest, ITenantScopedRequest
{
    public string TenantId { get; set; } = default!;   // injected
    public Guid TicketId   { get; init; }
}

public sealed class CompleteTicketCommandHandler
    : IRequestHandler<CompleteTicketCommand>
{
    private readonly ITicketRepository _repository;
    private readonly IDateTime _clock;

    public CompleteTicketCommandHandler(
        ITicketRepository repository,
        IDateTime clock)
    {
        _repository = repository;
        _clock      = clock;
    }

    public async Task<Unit> Handle(
        CompleteTicketCommand request,
        CancellationToken cancellationToken)
    {
        var ticket = await _repository.GetByIdAsync(request.TicketId, cancellationToken);

        if (ticket is null)
            throw new InvalidOperationException($"Ticket {request.TicketId} not found.");

        if (!string.Equals(ticket.TenantId, request.TenantId, StringComparison.Ordinal))
            throw new InvalidOperationException("Ticket does not belong to current tenant.");

        ticket.Complete(_clock.UtcNow);

        await _repository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
