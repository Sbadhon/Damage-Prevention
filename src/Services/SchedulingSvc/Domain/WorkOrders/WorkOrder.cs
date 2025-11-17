using SharedKernel;

namespace SchedulingSvc.Domain.WorkOrders;

public enum WorkOrderStatus
{
    Pending = 0,
    Assigned = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4
}

public sealed class WorkOrder : AggregateRoot<Guid>
{
    public string TenantId { get; private set; } = default!;
    public Guid TicketId   { get; private set; }

    public string WorkType { get; private set; } = default!;
    public string Address  { get; private set; } = default!;
    public double Lat      { get; private set; }
    public double Lon      { get; private set; }

    public string? CrewId  { get; private set; }

    public WorkOrderStatus Status { get; private set; }

    public DateTimeOffset CreatedAt   { get; private set; }
    public DateTimeOffset? AssignedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }

    private WorkOrder() { }

    private WorkOrder(
        Guid id,
        string tenantId,
        Guid ticketId,
        string workType,
        string address,
        double lat,
        double lon,
        DateTimeOffset createdAt)
    {
        Id       = id;
        TenantId = tenantId;
        TicketId = ticketId;
        WorkType = workType;
        Address  = address;
        Lat      = lat;
        Lon      = lon;
        CreatedAt = createdAt;
        Status    = WorkOrderStatus.Pending;
    }

    public static WorkOrder CreateFromTicket(
        string tenantId,
        Guid ticketId,
        string workType,
        string address,
        double lat,
        double lon,
        DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("TenantId is required.", nameof(tenantId));

        if (ticketId == Guid.Empty)
            throw new ArgumentException("TicketId is required.", nameof(ticketId));

        if (string.IsNullOrWhiteSpace(workType))
            throw new ArgumentException("WorkType is required.", nameof(workType));

        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Address is required.", nameof(address));

        return new WorkOrder(Guid.NewGuid(), tenantId, ticketId, workType, address, lat, lon, createdAt);
    }

    public void AssignCrew(string crewId, DateTimeOffset assignedAt)
    {
        if (Status is WorkOrderStatus.Completed or WorkOrderStatus.Cancelled)
            throw new InvalidOperationException("Cannot assign crew to completed or cancelled work orders.");

        if (string.IsNullOrWhiteSpace(crewId))
            throw new ArgumentException("CrewId is required.", nameof(crewId));

        CrewId     = crewId;
        Status     = WorkOrderStatus.Assigned;
        AssignedAt = assignedAt;
    }

    public void Complete(DateTimeOffset completedAt)
    {
        if (Status is not WorkOrderStatus.Assigned and not WorkOrderStatus.InProgress)
            throw new InvalidOperationException("Only assigned or in-progress work orders can be completed.");

        Status      = WorkOrderStatus.Completed;
        CompletedAt = completedAt;
    }

    public void Cancel(DateTimeOffset cancelledAt)
    {
        if (Status == WorkOrderStatus.Completed)
            throw new InvalidOperationException("Completed work orders cannot be cancelled.");

        Status      = WorkOrderStatus.Cancelled;
        CancelledAt = cancelledAt;
    }
}
