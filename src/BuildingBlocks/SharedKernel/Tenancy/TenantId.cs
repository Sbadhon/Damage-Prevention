namespace SharedKernel.Tenancy;

/// <summary>
/// Strongly-typed identifier for a tenant (customer / organization).
/// </summary>
public readonly record struct TenantId(string Value)
{
    public override string ToString() => Value;

    public static TenantId From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Tenant ID cannot be empty.", nameof(value));

        return new TenantId(value.Trim());
    }

    public static TenantId? TryFrom(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : new TenantId(value.Trim());
}
