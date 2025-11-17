using MediatR;
using TicketSvc.Application.Common.Tenancy;
using TicketSvc.Application.Tickets.Dtos;
using TicketSvc.Domain.Abstractions;

namespace TicketSvc.Application.Tickets.Queries;

public sealed class ListTicketsResult
{
    public required IReadOnlyList<TicketDto> Items { get; init; }
    public int TotalCount  { get; init; }
    public int PageNumber  { get; init; }
    public int PageSize    { get; init; }
}

public sealed class ListTicketsQuery
    : IRequest<ListTicketsResult>, ITenantScopedRequest
{
    // Filled by TenantBehavior
    public string TenantId { get; set; } = default!;

    // Paging
    public int PageNumber { get; init; } = 1;
    public int PageSize   { get; init; } = 20;
}

public sealed class ListTicketsQueryHandler
    : IRequestHandler<ListTicketsQuery, ListTicketsResult>
{
    private readonly ITicketRepository _repository;

    public ListTicketsQueryHandler(ITicketRepository repository)
    {
        _repository = repository;
    }

    public async Task<ListTicketsResult> Handle(
        ListTicketsQuery request,
        CancellationToken cancellationToken)
    {
        var (items, total) = await _repository.ListByTenantAsync(
            request.TenantId,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var dtos = items
            .Select(TicketDto.FromEntity)
            .ToList();

        return new ListTicketsResult
        {
            Items      = dtos,
            TotalCount = total,
            PageNumber = request.PageNumber,
            PageSize   = request.PageSize
        };
    }
}
