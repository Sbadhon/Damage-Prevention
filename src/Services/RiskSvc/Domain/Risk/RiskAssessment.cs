using SharedKernel;
using SharedKernel.Domain.Entities;

namespace RiskSvc.Domain.Risk;

public enum RiskLevel
{
    Low = 0,
    Medium = 1,
    High = 2
}

public sealed class RiskAssessment : AggregateRoot<Guid>
{
    public string TenantId { get; private set; } = default!;
    public Guid TicketId { get; private set; }

    public double Score { get; private set; }
    public RiskLevel Level { get; private set; }

    public string WorkType { get; private set; } = default!;
    public string Address { get; private set; } = default!;
    public double Lat { get; private set; }
    public double Lon { get; private set; }

    public DateTimeOffset AssessedAt { get; private set; }

    private RiskAssessment() { }

    private RiskAssessment(
        Guid id,
        string tenantId,
        Guid ticketId,
        double score,
        RiskLevel level,
        string workType,
        string address,
        double lat,
        double lon,
        DateTimeOffset assessedAt)
    {
        Id = id;
        TenantId = tenantId;
        TicketId = ticketId;
        Score = score;
        Level = level;
        WorkType = workType;
        Address = address;
        Lat = lat;
        Lon = lon;
        AssessedAt = assessedAt;
    }

    public static RiskAssessment Create(
        string tenantId,
        Guid ticketId,
        double score,
        RiskLevel level,
        string workType,
        string address,
        double lat,
        double lon,
        DateTimeOffset assessedAt)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("TenantId is required.", nameof(tenantId));

        if (ticketId == Guid.Empty)
            throw new ArgumentException("TicketId is required.", nameof(ticketId));

        return new RiskAssessment(
            Guid.NewGuid(),
            tenantId,
            ticketId,
            score,
            level,
            workType,
            address,
            lat,
            lon,
            assessedAt);
    }
}
