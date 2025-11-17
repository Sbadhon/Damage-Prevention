using RiskSvc.Domain.Risk;

namespace RiskSvc.Api.Contracts.Risk;

public sealed record RiskAssessmentResponse(
    Guid RiskId,
    Guid TicketId,
    double Score,
    RiskLevel Level,
    string WorkType,
    string Address,
    double Lat,
    double Lon,
    DateTimeOffset AssessedAt
);
