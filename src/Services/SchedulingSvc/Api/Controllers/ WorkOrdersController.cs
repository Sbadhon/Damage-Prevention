using MediatR;
using Microsoft.AspNetCore.Mvc;
using SchedulingSvc.Api.Contracts.WorkOrders;
using SchedulingSvc.Application.WorkOrders.Commands;
using SchedulingSvc.Application.WorkOrders.Queries;

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

    // POST api/workorders
    [HttpPost]
    public async Task<IActionResult> CreateWorkOrder(
        [FromBody] CreateWorkOrderRequest request,
        CancellationToken ct)
    {
        var cmd = new CreateWorkOrderCommand
        {
            TicketId = request.TicketId,
            WorkType = request.WorkType,
            Address = request.Address,
            Lat = request.Lat,
            Lon = request.Lon
        };

        var id = await _sender.Send(cmd, ct);
        return AcceptedAtAction(nameof(GetById), new { id }, new { WorkOrderId = id });
    }

    // GET api/workorders/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var query = new GetWorkOrderByIdQuery
        {
            WorkOrderId = id
        };

        var dto = await _sender.Send(query, ct);
        if (dto is null)
            return NotFound();

        var response = new WorkOrderResponse(
            WorkOrderId: dto.Id,
            TicketId: dto.TicketId,
            WorkType: dto.WorkType,
            Address: dto.Address,
            Lat: dto.Lat,
            Lon: dto.Lon,
            CrewId: dto.CrewId,
            Status: Enum.Parse<Domain.WorkOrders.WorkOrderStatus>(dto.Status));

        return Ok(response);
    }

    // GET api/workorders?pageNumber=1&pageSize=20
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = new ListWorkOrdersQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _sender.Send(query, ct);

        var items = result.Items.Select(dto => new WorkOrderResponse(
            WorkOrderId: dto.Id,
            TicketId: dto.TicketId,
            WorkType: dto.WorkType,
            Address: dto.Address,
            Lat: dto.Lat,
            Lon: dto.Lon,
            CrewId: dto.CrewId,
            Status: Enum.Parse<Domain.WorkOrders.WorkOrderStatus>(dto.Status)
        ));

        return Ok(new
        {
            items,
            result.TotalCount,
            result.PageNumber,
            result.PageSize
        });
    }

    // POST api/workorders/{id}/assign?crewId=crew-123
    [HttpPost("{id:guid}/assign")]
    public async Task<IActionResult> AssignCrew(
        Guid id,
        [FromQuery] string crewId,
        CancellationToken ct)
    {
        var cmd = new AssignCrewCommand
        {
            WorkOrderId = id,
            CrewId = crewId
        };

        await _sender.Send(cmd, ct);
        return NoContent();
    }

    // POST api/workorders/{id}/complete
    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id, CancellationToken ct)
    {
        var cmd = new CompleteWorkOrderCommand
        {
            WorkOrderId = id
        };

        await _sender.Send(cmd, ct);
        return NoContent();
    }

    // POST api/workorders/{id}/cancel
    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        var cmd = new CancelWorkOrderCommand
        {
            WorkOrderId = id
        };

        await _sender.Send(cmd, ct);
        return NoContent();
    }
}
