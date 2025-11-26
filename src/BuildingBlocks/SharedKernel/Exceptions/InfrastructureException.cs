namespace MyCompany.MyService.SharedKernel.Exceptions;
// When to use
// Inside Infrastructure layer only

/// <summary>
/// Used when underlying infrastructure components fail:
///  - Database connectivity
///  - Event bus failure
///  - Outbox persistence issues
///  - External API calls
///
/// API maps these to 503 (Service Unavailable).
/// </summary>
public class InfrastructureException : Exception
{
    public InfrastructureException(string message)
        : base(message)
    { }

    public InfrastructureException(string message, Exception? innerException)
        : base(message, innerException)
    { }
}
