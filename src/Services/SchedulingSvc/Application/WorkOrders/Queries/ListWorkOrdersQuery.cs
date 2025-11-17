using MediatR;
using SchedulingSvc.Application.Common.Tenancy;
using SchedulingSvc.Application.WorkOrders.Dtos;
using SchedulingSvc.Domain.Abstractions;

namespace SchedulingSvc.Application.WorkOrders.Queries;

public sealed class ListWorkOrdersResult
{
    public required IReadOnlyList<WorkOrderDto> Items { get; init; }
    public int TotalCount  { get; init; }
    public int PageNumber  { get; init; }
    public int PageSize    { get; init; }
}

public sealed class ListWorkOrdersQuery
    : IRequest<ListWorkOrdersResult>, ITenantScopedRequest
{
    public string TenantId { get; set; } = default!;
    public int PageNumber { get; init; } = 1;
    public int PageSize   { get; init; } = 20;
}

public sealed class ListWorkOrdersQueryHandler
    : IRequestHandler<ListWorkOrdersQuery, ListWorkOrdersResult>
{
    private readonly IWorkOrderRepository _repository;

    public ListWorkOrdersQueryHandler(IWorkOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<ListWorkOrdersResult> Handle(
        ListWorkOrdersQuery request,
        CancellationToken ct)
    {
        var (items, total) = await _repository.ListByTenantAsync(
            request.TenantId,
            request.PageNumber,
            request.PageSize,
            ct);

        var dtos = items.Select(WorkOrderDto.FromEntity).ToList();

        return new ListWorkOrdersResult
        {
            Items      = dtos,
            TotalCount = total,
            PageNumber = request.PageNumber,
            PageSize   = request.PageSize
        };
    }
}
