using SharedKernel.Domain.Entities;
using SharedKernel.Tenancy;

namespace TicketSvc.Domain.Tickets;

public enum TicketStatus
{
    Draft = 0,
    Submitted = 1,
    Assigned = 2,
    Completed = 3,
    Cancelled = 4
}

public sealed class Ticket : AggregateRoot<Guid>
{
    public TenantId TenantId { get; private set; }
    public string WorkType { get; private set; } = default!;
    public string Address { get; private set; } = default!;
    public string Description { get; init; } = default!;
    public double Lat { get; private set; }
    public double Lon { get; private set; }
    public string? CrewId { get; private set; }
    public TicketStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? SubmittedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }

    // For EF / serializers
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private Ticket() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    private Ticket(
        Guid id,
        TenantId tenantId,
        string workType,
        string address,
        string description,
        double lat,
        double lon,
        DateTimeOffset createdAt)
    {
        Id = id;
        TenantId = tenantId;
        WorkType = workType;
        Address = address;
        Description = description;
        Lat = lat;
        Lon = lon;
        CrewId = null;
        CreatedAt = createdAt;
        Status = TicketStatus.Draft;
    }

    public static Ticket CreateDraft(
        TenantId tenantId,
        string workType,
        string address,
        string description,
        double lat,
        double lon,
        DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(tenantId.Value))
            throw new ArgumentException("TenantId is required.", nameof(tenantId));

        if (string.IsNullOrWhiteSpace(workType))
            throw new ArgumentException("Work type is required.", nameof(workType));

        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Address is required.", nameof(address));

        return new Ticket(
            id: Guid.NewGuid(),
            tenantId: tenantId,
            workType: workType,
            address: address,
            description: description,
            lat: lat,
            lon: lon,
            createdAt: createdAt
        );
    }

    public void Submit(DateTimeOffset submittedAt)
    {
        if (Status != TicketStatus.Draft)
            throw new InvalidOperationException("Only draft tickets can be submitted.");

        Status = TicketStatus.Submitted;
        SubmittedAt = submittedAt;
    }

    public void MarkAssigned(string crewId)
    {
        if (Status is TicketStatus.Completed or TicketStatus.Cancelled)
            throw new InvalidOperationException("Cannot assign a closed ticket.");
        CrewId = crewId;
        Status = TicketStatus.Assigned;
    }
    public void OnWorkOrderCompleted()
    {
        if (Status is TicketStatus.Completed or TicketStatus.Cancelled)
            return; // Already closed, do nothing

        Status = TicketStatus.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    public void OnWorkOrderCancelled()
    {
        if (Status is TicketStatus.Completed or TicketStatus.Cancelled)
            return; // Already closed, do nothing

        Status = TicketStatus.Submitted; // revert to Submitted/Open
        SubmittedAt ??= DateTimeOffset.UtcNow;
    }

    public void Complete(DateTimeOffset completedAt)
    {
        if (Status is not TicketStatus.Submitted and not TicketStatus.Assigned)
            throw new InvalidOperationException("Only submitted or assigned tickets can be completed.");

        Status = TicketStatus.Completed;
        CompletedAt = completedAt;
    }

    public void Cancel(DateTimeOffset cancelledAt)
    {
        if (Status == TicketStatus.Completed)
            throw new InvalidOperationException("Completed tickets cannot be cancelled.");

        Status = TicketStatus.Cancelled;
        CancelledAt = cancelledAt;
    }
}
