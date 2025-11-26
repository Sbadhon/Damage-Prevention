using SharedKernel.Domain.ValueObjects;

namespace TicketSvc.Domain.Tickets;

/// <summary>
/// Strongly-typed identity for Ticket.
/// </summary>
public sealed class TicketId : ValueObject
{
    public Guid Value { get; }

    private TicketId(Guid value)
    {
        Value = value;
    }

    public static TicketId New()
        => new(Guid.NewGuid());

    public static TicketId From(Guid value)
        => new(value);

    public override string ToString()
        => Value.ToString();

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
