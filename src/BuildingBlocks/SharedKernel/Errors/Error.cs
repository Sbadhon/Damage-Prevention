namespace SharedKernel;

/// <summary>
/// Structured error that can travel through
/// Domain → Application → API without throwing exceptions.
/// </summary>
public sealed record Error(string Code, string Message, string? Details = null)
{
    /// <summary>
    /// Represents "no error".
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty);

    /// <summary>
    /// Convenience for comparing against "no error".
    /// </summary>
    public bool IsNone =>
        ReferenceEquals(this, None) ||
        (string.IsNullOrWhiteSpace(Code) && string.IsNullOrWhiteSpace(Message));

    public override string ToString()
        => Details is null or { Length: 0 }
            ? $"{Code}: {Message}"
            : $"{Code}: {Message} ({Details})";
}
