using System.ComponentModel;
using TicketSvc.Domain.Tickets;

namespace TicketSvc.Application.Tickets.Dtos;

public sealed class TicketDto
{
    public Guid Id { get; init; }
    public string TenantId { get; init; } = default!;
    public string WorkType { get; init; } = default!;
    public string Address  { get; init; } = default!;
    public string Description { get; init; } = default!;
    public double Lat { get; init; }
    public double Lon      { get; init; }
    public string Status   { get; init; } = default!;
    public DateTimeOffset CreatedAt    { get; init; }
    public DateTimeOffset? SubmittedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
    public DateTimeOffset? CancelledAt { get; init; }

    public static TicketDto FromEntity(Ticket ticket) =>
        new()
        {
            Id          = ticket.Id,
            TenantId    = ticket.TenantId,
            WorkType    = ticket.WorkType,
            Address     = ticket.Address,
            Description = ticket.Description,
            Lat         = ticket.Lat,
            Lon         = ticket.Lon,
            Status      = ticket.Status.ToString(),
            CreatedAt   = ticket.CreatedAt,
            SubmittedAt = ticket.SubmittedAt,
            CompletedAt = ticket.CompletedAt,
            CancelledAt = ticket.CancelledAt
        };
}
