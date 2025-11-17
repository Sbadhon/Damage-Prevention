using SchedulingSvc.Domain.WorkOrders;

namespace SchedulingSvc.Api.Contracts.WorkOrders;

public sealed record WorkOrderResponse(
    Guid WorkOrderId,
    Guid TicketId,
    string WorkType,
    string Address,
    double Lat,
    double Lon,
    string? CrewId,
    WorkOrderStatus Status
);
