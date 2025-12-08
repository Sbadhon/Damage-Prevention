using MediatR;
using Microsoft.AspNetCore.Mvc;
using SchedulingSvc.Api.Contracts.WorkOrders;
using SchedulingSvc.Application.Crew;
using SchedulingSvc.Application.WorkOrders.Commands;
using SchedulingSvc.Application.WorkOrders.Dtos;
using SchedulingSvc.Application.WorkOrders.Queries;
using SchedulingSvc.Api.Middleware;

namespace SchedulingSvc.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class WorkOrdersController : ControllerBase
{
    private readonly ISender _sender;

    public WorkOrdersController(ISender sender)
    {
        _sender = sender;
    }

    // GET api/workorders?pageNumber=1&pageSize=20
    [HttpGet]
    public async Task<ActionResult<PagedResponse<WorkOrderResponse>>> List(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (pageNumber <= 0) pageNumber = 1;
        if (pageSize <= 0 || pageSize > 100) pageSize = 20;

        var query = new ListWorkOrdersQuery
        {
            TenantId = HttpContext.GetTenantId(),
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        ListWorkOrdersResult result = await _sender.Send(query, ct);

        var items = result.Items.Select(ToResponse).ToList();

        return Ok(new PagedResponse<WorkOrderResponse>
        {
            Items = items,
            TotalCount = result.TotalCount,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize
        });
    }

    // GET api/workorders/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WorkOrderResponse>> GetById(Guid id, CancellationToken ct)
    {
        var query = new GetWorkOrderByIdQuery
        {
            TenantId = HttpContext.GetTenantId(),
            WorkOrderId = id
        };

        WorkOrderDto? dto = await _sender.Send(query, ct);

        if (dto is null)
            return NotFound();

        return Ok(ToResponse(dto));
    }

    [HttpPost]
    public async Task<ActionResult<WorkOrderResponse>> Create(
    [FromBody] CreateWorkOrderRequest request,
    CancellationToken ct)
    {
        var cmd = new CreateWorkOrderCommand
        {
            TicketId = request.TicketId,
            TenantId = HttpContext.GetTenantId(),
            WorkType = request.WorkType,
            Address = request.Address,
            Lat = request.Lat,
            Lon = request.Lon
        };

        WorkOrderDto dto = await _sender.Send(cmd, ct);
        var response = ToResponse(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = response.WorkOrderId },
            response);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<WorkOrderResponse>> UpdateStatus(
        Guid id,
        [FromBody] UpdateWorkOrderStatusRequest request,
        CancellationToken ct)
    {
        var cmd = new UpdateWorkOrderStatusCommand
        {
            WorkOrderId = id,
            Status = request.Status,
            TenantId = HttpContext.GetTenantId()
        };

        await _sender.Send(cmd, ct); // returns Unit

        // Get the updated DTO
        var query = new GetWorkOrderByIdQuery { WorkOrderId = id };
        WorkOrderDto? dto = await _sender.Send(query, ct);

        if (dto is null)
            return NotFound();

        var response = ToResponse(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = response.WorkOrderId },
            response
        );
    }

    [HttpGet("crews")]
    public async Task<ActionResult<CrewInfo[]>> ListCrews(CancellationToken ct)
    {
        var query = new ListCrewsQuery();
        var crews = await _sender.Send(query, ct);
        return Ok(crews);
    }

    [HttpPut("{id:guid}/assign-crew")]
    public async Task<ActionResult<WorkOrderResponse>> AssignCrew(
    Guid id,
    [FromBody] AssignCrewRequest request,
    CancellationToken ct)
    {
        var cmd = new AssignCrewCommand
        {
            WorkOrderId = id,
            CrewId = request.CrewId,
            CrewName = request.CrewName ?? string.Empty, // pass optional name if needed
            TenantId = HttpContext.GetTenantId()
        };

        await _sender.Send(cmd, ct);

        // Return updated WorkOrder
        var query = new GetWorkOrderByIdQuery { WorkOrderId = id };
        var dto = await _sender.Send(query, ct);

        if (dto is null)
            return NotFound();

        return Ok(ToResponse(dto));
    }

    private static WorkOrderResponse ToResponse(WorkOrderDto dto) =>
        new(
            WorkOrderId: dto.Id,
            TicketId: dto.TicketId,
            WorkType: dto.WorkType,
            Address: dto.Address,
            Lat: dto.Lat,
            Lon: dto.Lon,
            CrewId: dto.CrewId,
            CrewName: dto.CrewName,
            Status: dto.Status,
            Details: dto.Details,
            ScheduledAt: dto.ScheduledAt,
            CreatedAt: dto.CreatedAt
        );

    public sealed class PagedResponse<T>
    {
        public required IReadOnlyList<T> Items { get; init; }
        public int TotalCount { get; init; }
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
    }
}
