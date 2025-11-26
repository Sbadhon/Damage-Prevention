using MediatR;
using RiskSvc.Application.Common.Tenancy;
using RiskSvc.Application.Risk.Dtos;
using RiskSvc.Domain.Abstractions;

namespace RiskSvc.Application.Risk.Queries;

public sealed class ListRiskAssessmentsResult
{
    public required IReadOnlyList<RiskAssessmentDto> Items { get; init; }
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
}

public sealed class ListRiskAssessmentsQuery
    : IRequest<ListRiskAssessmentsResult>, ITenantScopedRequest
{
    public string TenantId { get; set; } = default!;
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed class ListRiskAssessmentsQueryHandler
    : IRequestHandler<ListRiskAssessmentsQuery, ListRiskAssessmentsResult>
{
    private readonly IRiskAssessmentRepository _repository;

    public ListRiskAssessmentsQueryHandler(IRiskAssessmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<ListRiskAssessmentsResult> Handle(
        ListRiskAssessmentsQuery request,
        CancellationToken ct)
    {
        var (items, total) = await _repository.ListByTenantAsync(
            request.TenantId,
            request.PageNumber,
            request.PageSize,
            ct);

        var dtos = items.Select(RiskAssessmentDto.FromEntity).ToList();

        return new ListRiskAssessmentsResult
        {
            Items = dtos,
            TotalCount = total,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
