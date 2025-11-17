using MediatR;
using SharedKernel;
using TicketSvc.Application.Common.Tenancy;
using TicketSvc.Domain.Abstractions;
using TicketSvc.Domain.Events;
using TicketSvc.Domain.Tickets;
using Contracts.Tickets;

namespace TicketSvc.Application.Tickets.Commands;

public sealed class SubmitTicketCommand : IRequest<Guid>, ITenantScopedRequest
{
    // Filled by TenantBehavior from ITenantProvider
    public string TenantId { get; set; } = default!;

    public string WorkType { get; init; } = default!;
    public string Address { get; init; } = default!;
    public string Description { get; init; } = default!;
    public double Lat { get; init; }
    public double Lon { get; init; }
}

public sealed class SubmitTicketCommandHandler : IRequestHandler<SubmitTicketCommand, Guid>
{
    private readonly ITicketRepository _tickets;
    private readonly ITicketEventPublisher _events;
    private readonly IDateTime _clock;

    public SubmitTicketCommandHandler(
        ITicketRepository tickets,
        ITicketEventPublisher events,
        IDateTime clock)
    {
        _tickets = tickets;
        _events = events;
        _clock = clock;
    }

    public async Task<Guid> Handle(SubmitTicketCommand request, CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;

        var ticket = Ticket.CreateDraft(
            request.TenantId,
            request.WorkType,
            request.Address,
            request.Description,
            request.Lat,
            request.Lon,
            now);

        ticket.Submit(now);

        await _tickets.AddAsync(ticket, cancellationToken);
        await _tickets.SaveChangesAsync(cancellationToken);

        var evt = new TicketSubmittedEvent(
            TenantId: request.TenantId,
            TicketId: ticket.Id,
            WorkType: ticket.WorkType,
            Address: ticket.Address,
            Description: ticket.Description,
            Lat: ticket.Lat,
            Lon: ticket.Lon,
            SubmittedAt: now
        );


        await _events.PublishTicketSubmittedAsync(ticket, evt, cancellationToken);

        return ticket.Id;
    }
}
