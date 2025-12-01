namespace Contracts.Tickets;

public sealed record TicketCancelledEvent(
    string TenantId,
    Guid TicketId,
    DateTimeOffset CanceledAt,
    string? Reason
);
