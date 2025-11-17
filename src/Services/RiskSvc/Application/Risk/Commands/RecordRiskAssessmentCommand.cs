using MediatR;
using SharedKernel;
using RiskSvc.Application.Common.Tenancy;
using RiskSvc.Application.Risk.Dtos;
using RiskSvc.Domain.Abstractions;
using RiskSvc.Domain.Risk;

namespace RiskSvc.Application.Risk.Commands;

public sealed class RecordRiskAssessmentCommand
    : IRequest<RiskAssessmentDto>, ITenantScopedRequest
{
    public string TenantId { get; set; } = default!;

    public Guid TicketId { get; init; }
    public string WorkType { get; init; } = default!;
    public string Address { get; init; } = default!;
    public double Lat { get; init; }
    public double Lon { get; init; }
}

public sealed class RecordRiskAssessmentCommandHandler
    : IRequestHandler<RecordRiskAssessmentCommand, RiskAssessmentDto>
{
    private readonly IRiskAssessmentRepository _repository;
    private readonly IDateTime _clock;

    public RecordRiskAssessmentCommandHandler(
        IRiskAssessmentRepository repository,
        IDateTime clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public async Task<RiskAssessmentDto> Handle(
        RecordRiskAssessmentCommand request,
        CancellationToken ct)
    {
        // Simple dumb scoring example for now:
        // - base score by work type
        // - maybe tweak by geo in future.

        var baseScore = request.WorkType.ToLowerInvariant() switch
        {
            var w when w.Contains("blast") || w.Contains("explosive") => 0.9,
            var w when w.Contains("drill") || w.Contains("boring") => 0.7,
            var w when w.Contains("fiber") => 0.5,
            _ => 0.4
        };


        var score = baseScore; // extend later with historical data, soil type, depth, etc.

        var level =
            score >= 0.8 ? RiskLevel.High :
            score >= 0.6 ? RiskLevel.Medium :
                           RiskLevel.Low;

        var now = _clock.UtcNow;

        var entity = RiskAssessment.Create(
            request.TenantId,
            request.TicketId,
            score,
            level,
            request.WorkType,
            request.Address,
            request.Lat,
            request.Lon,
            now);

        await _repository.AddAsync(entity, ct);
        await _repository.SaveChangesAsync(ct);

        return RiskAssessmentDto.FromEntity(entity);
    }
}
