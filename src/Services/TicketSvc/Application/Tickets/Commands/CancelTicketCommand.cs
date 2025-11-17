using MediatR;
using SharedKernel;
using TicketSvc.Application.Common.Tenancy;
using TicketSvc.Domain.Abstractions;

namespace TicketSvc.Application.Tickets.Commands;

public sealed class CancelTicketCommand : IRequest, ITenantScopedRequest
{
    public string TenantId { get; set; } = default!;
    public Guid TicketId   { get; init; }

    // Optional: reason for auditing/logging (not used in domain yet)
    public string? Reason  { get; init; }
}

public sealed class CancelTicketCommandHandler
    : IRequestHandler<CancelTicketCommand>
{
    private readonly ITicketRepository _repository;
    private readonly IDateTime _clock;

    public CancelTicketCommandHandler(
        ITicketRepository repository,
        IDateTime clock)
    {
        _repository = repository;
        _clock      = clock;
    }

    public async Task<Unit> Handle(
        CancelTicketCommand request,
        CancellationToken cancellationToken)
    {
        var ticket = await _repository.GetByIdAsync(request.TicketId, cancellationToken);

        if (ticket is null)
            throw new InvalidOperationException($"Ticket {request.TicketId} not found.");

        if (!string.Equals(ticket.TenantId, request.TenantId, StringComparison.Ordinal))
            throw new InvalidOperationException("Ticket does not belong to current tenant.");

        ticket.Cancel(_clock.UtcNow);

        await _repository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
