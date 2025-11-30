using System.ComponentModel.DataAnnotations;

namespace TicketSvc.Api.Contracts.Tickets;

public sealed record SubmitTicketRequest
{
    public string WorkType { get; set; } = default!;

    public string Address { get; set; } = default!;

    public string Description { get; set; } = default!;

    public double Lat { get; init; }

    public double Lon { get; init; }
}
