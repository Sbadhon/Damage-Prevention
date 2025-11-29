using System.ComponentModel;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TicketSvc.Api.Contracts.Tickets;
using TicketSvc.Application.Tickets.Commands;
using TicketSvc.Application.Tickets.Queries;
using TicketSvc.Domain.Tickets;
using TicketSvc.Api.Middleware;

namespace TicketSvc.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TicketsController : ControllerBase
{
    private readonly ISender _sender;

    public TicketsController(ISender sender)
    {
        _sender = sender;
    }

    // POST api/tickets
    [HttpPost]
    public async Task<IActionResult> SubmitTicket(
        [FromBody] SubmitTicketRequest request,
        CancellationToken ct)
    {
        var command = new SubmitTicketCommand
        {
            TenantId = HttpContext.GetTenantId(),
            WorkType = request.WorkType,
            Address = request.Address,
            Description = request.Description,
            Lat = request.Lat,
            Lon = request.Lon
        };

        // Create the ticket
        var id = await _sender.Send(command, ct);

        // Fetch the full DTO for the created ticket
        var query = new GetTicketByIdQuery { TicketId = id };
        var dto = await _sender.Send(query, ct);

        if (dto is null)
            return NotFound();

        // Return the full response
        var response = new TicketResponse(
            TicketId: dto.Id,
            WorkType: dto.WorkType,
            Address: dto.Address,
            Description: dto.Description,
            Lat: dto.Lat,
            Lon: dto.Lon,
            Status: dto.Status,
            CreatedAt: dto.CreatedAt,
            SubmittedAt: dto.SubmittedAt,
            CompletedAt: dto.CompletedAt,
            CancelledAt: dto.CancelledAt
        );

        return Ok(response);
    }

    // GET api/tickets/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTicketById(Guid id, CancellationToken ct)
    {
        var query = new GetTicketByIdQuery
        {
            TicketId = id,
            TenantId = HttpContext.GetTenantId(),
        };

        var dto = await _sender.Send(query, ct);

        if (dto is null)
            return NotFound();

        var response = new TicketResponse(
            TicketId: dto.Id,
            WorkType: dto.WorkType,
            Address: dto.Address,
            Description: dto.Description,
            Lat: dto.Lat,
            Lon: dto.Lon,
            Status: dto.Status,
            CreatedAt: dto.CreatedAt,
            SubmittedAt: dto.SubmittedAt,
            CompletedAt: dto.CompletedAt,
            CancelledAt: dto.CancelledAt
        );

        return Ok(response);
    }

    // GET api/tickets?pageNumber=1&pageSize=20
    [HttpGet]
    public async Task<IActionResult> ListTickets(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = new ListTicketsQuery
        {
            TenantId = HttpContext.GetTenantId(),
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _sender.Send(query, ct);

        var items = result.Items
            .Select(dto => new TicketResponse(
                TicketId: dto.Id,
                WorkType: dto.WorkType,
                Address: dto.Address,
                Description: dto.Description,
                Lat: dto.Lat,
                Lon: dto.Lon,
                Status: dto.Status,
                CreatedAt: dto.CreatedAt,
                SubmittedAt: dto.SubmittedAt,
                CompletedAt: dto.CompletedAt,
                CancelledAt: dto.CancelledAt
            ))
            .ToList();

        var response = new
        {
            items,
            result.TotalCount,
            result.PageNumber,
            result.PageSize
        };

        return Ok(response);
    }

    // POST api/tickets/{id}/complete
    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> CompleteTicket(Guid id, CancellationToken ct)
    {
        var command = new CompleteTicketCommand
        {
            TicketId = id
        };

        await _sender.Send(command, ct);

       // Fetch the full DTO for the created ticket
        var query = new GetTicketByIdQuery { TicketId = id };
        var dto = await _sender.Send(query, ct);

        if (dto is null)
            return NotFound();

        // Return the full response
        var response = new TicketResponse(
            TicketId: dto.Id,
            WorkType: dto.WorkType,
            Address: dto.Address,
            Description: dto.Description,
            Lat: dto.Lat,
            Lon: dto.Lon,
            Status: dto.Status,
            CreatedAt: dto.CreatedAt,
            SubmittedAt: dto.SubmittedAt,
            CompletedAt: dto.CompletedAt,
            CancelledAt: dto.CancelledAt
        );

        return Ok(response);
    }

    // POST api/tickets/{id}/cancel
    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> CancelTicket(
        Guid id,
        [FromBody] string? reason,
        CancellationToken ct)
    {
        var command = new CancelTicketCommand
        {
            TicketId = id,
            Reason = reason
        };

        await _sender.Send(command, ct);

        // Fetch the full DTO for the created ticket
        var query = new GetTicketByIdQuery { TicketId = id };
        var dto = await _sender.Send(query, ct);

        if (dto is null)
            return NotFound();

        // Return the full response
        var response = new TicketResponse(
            TicketId: dto.Id,
            WorkType: dto.WorkType,
            Address: dto.Address,
            Description: dto.Description,
            Lat: dto.Lat,
            Lon: dto.Lon,
            Status: dto.Status,
            CreatedAt: dto.CreatedAt,
            SubmittedAt: dto.SubmittedAt,
            CompletedAt: dto.CompletedAt,
            CancelledAt: dto.CancelledAt
        );

        return Ok(response);
    }
}
