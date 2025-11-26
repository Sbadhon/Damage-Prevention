namespace SharedKernel.Tenancy;

/// <summary>
/// Abstraction for accessing the current tenant in the application.
/// Each service will provide its own implementation (from HTTP, message headers, etc.).
/// </summary>
public interface ITenantContext
{
    /// <summary>
    /// The current tenant, or null if this operation is not tenant-scoped.
    /// </summary>
    TenantId? CurrentTenant { get; }

    /// <summary>
    /// True if a tenant has been resolved for the current execution context.
    /// </summary>
    bool HasTenant => CurrentTenant is not null;
}
