namespace SharedKernel.Domain.Events;

/// <summary>
/// Base type for domain events with timestamp.
/// </summary>
public abstract class DomainEvent : IDomainEvent
{
    public DateTime OccurredOnUtc { get; protected init; }

    protected DomainEvent()
    {
        OccurredOnUtc = DateTime.UtcNow;
    }

    protected DomainEvent(DateTime occurredOnUtc)
    {
        OccurredOnUtc = occurredOnUtc;
    }
}
