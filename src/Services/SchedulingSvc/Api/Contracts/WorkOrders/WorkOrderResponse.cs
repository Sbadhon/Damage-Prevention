namespace SchedulingSvc.Api.Contracts.WorkOrders;

public sealed record WorkOrderResponse(
    Guid WorkOrderId,
    Guid TicketId,
    string WorkType,
    string Address,
    double Lat,
    double Lon,
    string? CrewId,
    string? CrewName,
    string Status,
    string Details,
    DateTimeOffset ScheduledAt,
    DateTimeOffset CreatedAt
);
