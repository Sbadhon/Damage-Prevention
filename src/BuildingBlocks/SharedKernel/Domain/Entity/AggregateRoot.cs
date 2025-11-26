using SharedKernel.Domain.Events;

namespace SharedKernel.Domain.Entities;

/// <summary>
/// Base type for aggregate roots that can raise domain events.
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId>, IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = new();

    protected AggregateRoot()
    {
    }

    protected AggregateRoot(TId id) : base(id)
    {
    }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
