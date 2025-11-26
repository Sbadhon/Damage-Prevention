using MediatR;
using RiskSvc.Application.Common.Tenancy;
using RiskSvc.Application.Risk.Dtos;
using RiskSvc.Domain.Abstractions;

namespace RiskSvc.Application.Risk.Queries;

public sealed class GetRiskByTicketIdQuery
    : IRequest<RiskAssessmentDto?>, ITenantScopedRequest
{
    public string TenantId { get; set; } = default!;
    public Guid TicketId { get; init; }
}

public sealed class GetRiskByTicketIdQueryHandler
    : IRequestHandler<GetRiskByTicketIdQuery, RiskAssessmentDto?>
{
    private readonly IRiskAssessmentRepository _repository;

    public GetRiskByTicketIdQueryHandler(IRiskAssessmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<RiskAssessmentDto?> Handle(
        GetRiskByTicketIdQuery request,
        CancellationToken ct)
    {
        var risk = await _repository.GetByTicketIdAsync(request.TicketId, ct);

        if (risk is null)
            return null;

        if (!string.Equals(risk.TenantId, request.TenantId, StringComparison.Ordinal))
            return null;

        return RiskAssessmentDto.FromEntity(risk);
    }
}
