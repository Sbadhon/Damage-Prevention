using SchedulingSvc.Domain.WorkOrders;

namespace SchedulingSvc.Application.WorkOrders.Dtos;

public sealed class WorkOrderDto
{
    public Guid Id { get; init; }
    public string TenantId { get; init; } = default!;
    public Guid TicketId { get; init; }

    public string WorkType { get; init; } = default!;
    public string Address { get; init; } = default!;
    public double Lat { get; init; }
    public double Lon { get; init; }

    public string Status { get; init; } = default!;

    public string? CrewId { get; init; }
    public string? CrewName { get; init; }
    public string Details { get; init; } = default!;

    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset ScheduledAt { get; init; }

    public DateTimeOffset? AssignedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
    public DateTimeOffset? CancelledAt { get; init; }

    public static WorkOrderDto FromEntity(WorkOrder wo)
    {
        var scheduledAt = wo.AssignedAt ?? wo.CreatedAt;

        return new WorkOrderDto
        {
            Id        = wo.Id,
            TenantId  = wo.TenantId,
            TicketId  = wo.TicketId,
            WorkType  = wo.WorkType,
            Address   = wo.Address,
            Lat       = wo.Lat,
            Lon       = wo.Lon,
            Status    = MapStatus(wo.Status),
            CrewId    = wo.CrewId,
            CrewName  = string.IsNullOrWhiteSpace(wo.CrewId)
                ? null
                : $"Crew {wo.CrewId}",
            Details     = $"{wo.WorkType} at {wo.Address}",
            CreatedAt   = wo.CreatedAt,
            ScheduledAt = scheduledAt,
            AssignedAt  = wo.AssignedAt,
            CompletedAt = wo.CompletedAt,
            CancelledAt = wo.CancelledAt
        };
    }

    private static string MapStatus(WorkOrderStatus status) =>
        status switch
        {
            WorkOrderStatus.Pending    => "Pending",
            WorkOrderStatus.Assigned   => "Assigned",
            WorkOrderStatus.InProgress => "Assigned",  // FE only knows Pending/Assigned/Completed/Cancelled
            WorkOrderStatus.Completed  => "Completed",
            WorkOrderStatus.Cancelled  => "Cancelled",
            _                          => "Pending"
        };

    public static implicit operator WorkOrderDto(Guid v)
    {
        throw new NotImplementedException();
    }
}
