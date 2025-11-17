using SchedulingSvc.Domain.WorkOrders;

namespace SchedulingSvc.Application.WorkOrders.Dtos;

public sealed class WorkOrderDto
{
    public Guid Id { get; init; }
    public string TenantId { get; init; } = default!;
    public Guid TicketId   { get; init; }
    public string WorkType { get; init; } = default!;
    public string Address  { get; init; } = default!;
    public double Lat      { get; init; }
    public double Lon      { get; init; }
    public string Status   { get; init; } = default!;
    public string? CrewId  { get; init; }
    public DateTimeOffset CreatedAt   { get; init; }
    public DateTimeOffset? AssignedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
    public DateTimeOffset? CancelledAt { get; init; }

    public static WorkOrderDto FromEntity(WorkOrder wo) =>
        new()
        {
            Id          = wo.Id,
            TenantId    = wo.TenantId,
            TicketId    = wo.TicketId,
            WorkType    = wo.WorkType,
            Address     = wo.Address,
            Lat         = wo.Lat,
            Lon         = wo.Lon,
            Status      = wo.Status.ToString(),
            CrewId      = wo.CrewId,
            CreatedAt   = wo.CreatedAt,
            AssignedAt  = wo.AssignedAt,
            CompletedAt = wo.CompletedAt,
            CancelledAt = wo.CancelledAt
        };
}
