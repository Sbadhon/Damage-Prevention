using System.Text.Json;
using MediatR;
using SharedKernel;
using SharedKernel.Tenancy;
using TicketSvc.Application.Common.Tenancy;
using TicketSvc.Domain.Abstractions;
using TicketSvc.Domain.Outbox;
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

public sealed class SubmitTicketCommandHandler
    : IRequestHandler<SubmitTicketCommand, Guid>
{
    private readonly ITicketRepository _tickets;
    private readonly IOutboxRepository _outbox;
    private readonly IDateTime _clock;

    public SubmitTicketCommandHandler(
        ITicketRepository tickets,
        IOutboxRepository outbox,
        IDateTime clock)
    {
        _tickets = tickets;
        _outbox = outbox;
        _clock = clock;
    }

    public async Task<Guid> Handle(
        SubmitTicketCommand request,
        CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;
        var ticket = Ticket.CreateDraft(
            new TenantId(request.TenantId),
            request.WorkType,
            request.Address,
            request.Description,
            request.Lat,
            request.Lon,
            now
        );

        await _tickets.AddAsync(ticket, cancellationToken);

        // TicketSubmittedEvent is a positional ctor:
        // TicketSubmittedEvent(string tenantId, Guid ticketId, string workType,
        //                      string address, string description, double lat, double lon, DateTimeOffset submittedAt)
        var evt = new TicketSubmittedEvent(
            ticket.TenantId.Value,
            ticket.Id,
            ticket.WorkType,
            ticket.Address,
            ticket.Description,
            ticket.Lat,
            ticket.Lon,
            now
        );

        // Serialize event for Outbox
        var payload = JsonSerializer.Serialize(evt);

        // OutboxMessage – we’ll fix its signature in the next step
        var outboxMessage = OutboxMessage.Create(
            ticketId: ticket.Id,
            tenantId: ticket.TenantId.Value,
            type: nameof(TicketSubmittedEvent),
            payload: payload,
            occurredAt: now
        );

        await _outbox.AddAsync(outboxMessage, cancellationToken);

        // No direct publish here; OutboxDispatcher will publish later
        return ticket.Id;
    }
}
