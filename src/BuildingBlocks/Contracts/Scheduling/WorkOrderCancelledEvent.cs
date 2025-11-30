using System;

namespace Contracts.Scheduling
{
    public record WorkOrderCancelledEvent
    {
        public Guid WorkOrderId { get; init; }
        public Guid TicketId { get; init; }
        public DateTimeOffset CancelledAt { get; init; }
    }
}
