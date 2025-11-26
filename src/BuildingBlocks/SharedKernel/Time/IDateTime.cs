namespace SharedKernel;

/// <summary>
/// Abstraction over system clock so we can test time-dependent logic
/// and keep domain code deterministic.
/// </summary>
public interface IDateTime
{
    DateTime UtcNow { get; }
}

/// <summary>
/// Default implementation that just uses DateTime.UtcNow.
/// </summary>
public sealed class SystemClock : IDateTime
{
    public DateTime UtcNow => DateTime.UtcNow;
}
