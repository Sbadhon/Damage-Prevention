namespace SharedKernel.Domain.ValueObjects;

/// <summary>
/// Base type for immutable value objects.
/// Implements equality based on atomic components defined by subclasses.
/// </summary>
public abstract class ValueObject
{
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
        {
            return false;
        }

        var other = (ValueObject)obj;

        return GetEqualityComponents()
            .SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Aggregate(
                0,
                (hash, component) =>
                {
                    var componentHash = component?.GetHashCode() ?? 0;
                    return HashCode.Combine(hash, componentHash);
                });
    }

    public static bool operator ==(ValueObject? left, ValueObject? right)
        => Equals(left, right);

    public static bool operator !=(ValueObject? left, ValueObject? right)
        => !Equals(left, right);
}
