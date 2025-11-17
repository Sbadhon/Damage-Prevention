using System.ComponentModel.DataAnnotations;

namespace TicketSvc.Api.Contracts.Tickets;

public sealed record SubmitTicketRequest
{
    [Required]
    public string WorkType { get; init; }

    [Required]
    public string Address { get; init; }

    [Required]
    public string Description { get; init; }

    [Range(-90, 90)]
    public double Lat { get; init; }

    [Range(-180, 180)]
    public double Lon { get; init; }

    public SubmitTicketRequest(string workType, string address, string description, double lat, double lon) =>
        (WorkType, Address, Description, Lat, Lon) = (workType, address, description, lat, lon);
}
