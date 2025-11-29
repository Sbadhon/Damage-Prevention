using MediatR;
using SchedulingSvc.Application.Common.Tenancy;
using SchedulingSvc.Application.WorkOrders.Dtos;
using SchedulingSvc.Domain.Abstractions;
using SchedulingSvc.Application.Crew;

namespace SchedulingSvc.Application.WorkOrders.Queries;

public sealed class ListCrewsQuery
    : IRequest<CrewInfo[]>, ITenantScopedRequest
{
    public string TenantId { get; set; } = default!;
}

public sealed class ListCrewsQueryHandler
    : IRequestHandler<ListCrewsQuery, CrewInfo[]>
{
    public Task<CrewInfo[]> Handle(ListCrewsQuery request, CancellationToken ct)
    {
        // No DB access — static crew directory
        return Task.FromResult(CrewDirectory.GetAll());
    }
}