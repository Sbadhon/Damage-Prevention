namespace MyCompany.MyService.SharedKernel.Exceptions;

// When to use
// Inside Application layer (CQRS handlers, services), not Domain

/// <summary>
/// Represents errors in the Application layer (CQRS use case orchestration).
///
/// Examples:
///  - Handler misconfiguration
///  - Unexpected nulls
///  - Business workflow failures
///
/// These map to 400 or 422 at the API layer.
/// </summary>
public class ApplicationLayerException : Exception
{
    public ApplicationLayerException(string message)
        : base(message)
    { }

    public ApplicationLayerException(string message, Exception? innerException)
        : base(message, innerException)
    { }
}
