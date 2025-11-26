namespace SharedKernel.Domain.Events;

/// <summary>
/// Indicates that an aggregate root or entity exposes domain events.
/// </summary>
public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}
