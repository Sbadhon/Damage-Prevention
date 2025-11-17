using RiskSvc.Domain.Risk;

namespace RiskSvc.Application.Risk.Dtos;

public sealed class RiskAssessmentDto
{
    public Guid Id { get; init; }
    public string TenantId { get; init; } = default!;
    public Guid TicketId   { get; init; }
    public double Score    { get; init; }
    public string Level    { get; init; } = default!;
    public string WorkType { get; init; } = default!;
    public string Address  { get; init; } = default!;
    public double Lat      { get; init; }
    public double Lon      { get; init; }
    public DateTimeOffset AssessedAt { get; init; }

    public static RiskAssessmentDto FromEntity(RiskAssessment risk) =>
        new()
        {
            Id         = risk.Id,
            TenantId   = risk.TenantId,
            TicketId   = risk.TicketId,
            Score      = risk.Score,
            Level      = risk.Level.ToString(),
            WorkType   = risk.WorkType,
            Address    = risk.Address,
            Lat        = risk.Lat,
            Lon        = risk.Lon,
            AssessedAt = risk.AssessedAt
        };
}
