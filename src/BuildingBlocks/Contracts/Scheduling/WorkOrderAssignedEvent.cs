namespace Contracts.Scheduling;

public record WorkOrderAssignedEvent(
    Guid WorkOrderId,
    Guid TicketId,
    string CrewId,
    string CrewName,
    DateTimeOffset AssignedAt
);
