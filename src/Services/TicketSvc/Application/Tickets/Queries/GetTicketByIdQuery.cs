using MediatR;
using TicketSvc.Application.Common.Tenancy;
using TicketSvc.Application.Tickets.Dtos;
using TicketSvc.Domain.Abstractions;

namespace TicketSvc.Application.Tickets.Queries;

public sealed class GetTicketByIdQuery
    : IRequest<TicketDto?>, ITenantScopedRequest
{
    // Filled by TenantBehavior
    public string TenantId { get; set; } = default!;
    public Guid TicketId { get; init; }
}

public sealed class GetTicketByIdQueryHandler
    : IRequestHandler<GetTicketByIdQuery, TicketDto?>
{
    private readonly ITicketRepository _repository;

    public GetTicketByIdQueryHandler(ITicketRepository repository)
    {
        _repository = repository;
    }

    public async Task<TicketDto?> Handle(
        GetTicketByIdQuery request,
        CancellationToken cancellationToken)
    {
        var ticket = await _repository.GetByIdAsync(request.TicketId, cancellationToken);

        if (ticket is null)
            return null;

        // Enforce tenant isolation
        if (!string.Equals(ticket.TenantId, request.TenantId, StringComparison.Ordinal))
            return null;

        return TicketDto.FromEntity(ticket);
    }
}
