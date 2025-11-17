namespace Contracts.Tickets;

public sealed record TicketSubmittedEvent(
    string TenantId,
    Guid TicketId,
    string WorkType,
    string Address,
    string Description,
    double Lat,
    double Lon,
    DateTimeOffset SubmittedAt
);
