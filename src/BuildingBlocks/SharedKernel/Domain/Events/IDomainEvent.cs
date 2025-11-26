namespace SharedKernel.Domain.Events;

/// <summary>
/// Marker interface for domain events.
/// </summary>
public interface IDomainEvent
{
    DateTime OccurredOnUtc { get; }
}
