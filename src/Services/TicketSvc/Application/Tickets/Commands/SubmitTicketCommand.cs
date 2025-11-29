using System.Text.Json;
using MediatR;
using SharedKernel;
using TicketSvc.Application.Common.Tenancy;
using TicketSvc.Domain.Abstractions;
using TicketSvc.Domain.Outbox;
using TicketSvc.Domain.Tickets;
using Contracts.Tickets;
using SharedKernel.Tenancy;

namespace TicketSvc.Application.Tickets.Commands;

public sealed class SubmitTicketCommand : IRequest<Guid>, ITenantScopedRequest
{
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

        // Create as Draft
        var tenantId = new TenantId(request.TenantId);

        var ticket = Ticket.CreateDraft(
            tenantId,
            request.WorkType,
            request.Address,
            request.Description,
            request.Lat,
            request.Lon,
            now
        );

        // Immediately submit it (moves status + SubmittedAt)
        ticket.Submit(now);

        // 3️Persist aggregate
        await _tickets.AddAsync(ticket, cancellationToken);

        // Build integration event from the *submitted* ticket
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

        var payload = JsonSerializer.Serialize(evt);

        var outboxMessage = OutboxMessage.Create(
            ticketId: ticket.Id,
            tenantId: ticket.TenantId.Value,
            type: nameof(TicketSubmittedEvent),
            payload: payload,
            occurredAt: now
        );

        await _outbox.AddAsync(outboxMessage, cancellationToken);

        return ticket.Id;
    }
}
