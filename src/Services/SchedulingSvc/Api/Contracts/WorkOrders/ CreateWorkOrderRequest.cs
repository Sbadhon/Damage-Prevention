namespace SchedulingSvc.Api.Contracts.WorkOrders;

public sealed record CreateWorkOrderRequest(
    Guid TicketId,
    string WorkType,
    string Address,
    double Lat,
    double Lon
);
