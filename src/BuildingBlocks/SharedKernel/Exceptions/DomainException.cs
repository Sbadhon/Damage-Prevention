namespace MyCompany.MyService.SharedKernel.Exceptions;
// When to use
// In Domain layer only —> inside aggregates or domain services

/// <summary>
/// Thrown when a domain invariant is violated.
/// Examples:
///  - Renaming an inactive aggregate
///  - Negative quantity not allowed
///  - Invalid state transition
///
/// These are converted to 400 (BadRequest) by API middleware.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message)
        : base(message)
    { }

    public DomainException(string message, Exception? innerException)
        : base(message, innerException)
    { }
}
