using System;

namespace Contracts.Scheduling
{
    public record WorkOrderCompletedEvent
    {
        public Guid WorkOrderId { get; init; }
        public Guid TicketId { get; init; }
        public DateTimeOffset CompletedAt { get; init; }
    }
}
