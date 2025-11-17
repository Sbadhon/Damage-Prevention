namespace SharedKernel;

// Base entity with typed Id
public abstract class Entity<TId>
{
    public TId Id { get; protected set; } = default!;

    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public override int GetHashCode() => HashCode.Combine(Id);
}

// Aggregate root marker
public abstract class AggregateRoot<TId> : Entity<TId>
{
}

// Clock abstraction (helps a LOT for BDD/unit tests)
public interface IDateTime
{
    DateTime UtcNow { get; }
}

public sealed class SystemClock : IDateTime
{
    public DateTime UtcNow => DateTime.UtcNow;
}
