namespace TicketSvc.Api.Contracts.Tickets;

public sealed record TicketResponse(
    Guid TicketId,
    string WorkType,
    string Address,
    string Description,
    double Lat,
    double Lon,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? SubmittedAt,
    DateTimeOffset? CompletedAt,
    DateTimeOffset? CancelledAt
);
