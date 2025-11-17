using MediatR;
using Microsoft.AspNetCore.Mvc;
using RiskSvc.Api.Contracts.Risk;
using RiskSvc.Application.Risk.Queries;
using RiskSvc.Domain.Risk;

namespace RiskSvc.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class RiskController : ControllerBase
{
    private readonly ISender _sender;

    public RiskController(ISender sender)
    {
        _sender = sender;
    }

    // GET api/risk/ticket/{ticketId}
    [HttpGet("ticket/{ticketId:guid}")]
    public async Task<IActionResult> GetByTicketId(Guid ticketId, CancellationToken ct)
    {
        var query = new GetRiskByTicketIdQuery
        {
            TicketId = ticketId
        };

        var dto = await _sender.Send(query, ct);

        if (dto is null)
            return NotFound();

        var response = new RiskAssessmentResponse(
            RiskId: dto.Id,
            TicketId: dto.TicketId,
            Score: dto.Score,
            Level: Enum.Parse<RiskLevel>(dto.Level),
            WorkType: dto.WorkType,
            Address: dto.Address,
            Lat: dto.Lat,
            Lon: dto.Lon,
            AssessedAt: dto.AssessedAt);

        return Ok(response);
    }

    // GET api/risk?pageNumber=1&pageSize=20
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = new ListRiskAssessmentsQuery
        {
            PageNumber = pageNumber,
            PageSize   = pageSize
        };

        var result = await _sender.Send(query, ct);

        var items = result.Items.Select(dto => new RiskAssessmentResponse(
            RiskId: dto.Id,
            TicketId: dto.TicketId,
            Score: dto.Score,
            Level: Enum.Parse<RiskLevel>(dto.Level),
            WorkType: dto.WorkType,
            Address: dto.Address,
            Lat: dto.Lat,
            Lon: dto.Lon,
            AssessedAt: dto.AssessedAt
        ));

        return Ok(new
        {
            items,
            result.TotalCount,
            result.PageNumber,
            result.PageSize
        });
    }
}
