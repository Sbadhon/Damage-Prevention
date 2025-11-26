using SharedKernel.Domain.Entities;

namespace SchedulingSvc.Domain.Assignments;

public enum AssignmentStatus
{
    Pending = 0,
    Assigned = 1,
    Completed = 2,
    Cancelled = 3
}

public sealed class WorkAssignment : AggregateRoot<Guid>
{
    public Guid TicketId { get; private set; }
    public string Region { get; private set; } = default!;
    public string Crew { get; private set; } = default!;
    public AssignmentStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public Exception? Id { get; internal set; }

    private WorkAssignment() { }

    private WorkAssignment(Guid id, Guid ticketId, string region, string crew, DateTimeOffset createdAt)
    {
        TicketId = ticketId;
        Region = region;
        Crew = crew;
        CreatedAt = createdAt;
        Status = AssignmentStatus.Pending;
    }

    public static WorkAssignment CreateForTicket(Guid ticketId, string region, string crew, DateTimeOffset createdAt)
        => new(Guid.NewGuid(), ticketId, region, crew, createdAt);

    public void MarkAssigned()
    {
        if (Status != AssignmentStatus.Pending)
            throw new InvalidOperationException("Only pending assignments can be marked assigned.");

        Status = AssignmentStatus.Assigned;
    }
}

public class AggregateRoot<T>
{
}