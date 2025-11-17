using Contracts.Tickets;
using TicketSvc.Domain.Events;
using TicketSvc.Domain.Tickets;

namespace TicketSvc.Infrastructure.Events;

public sealed class NoOpTicketEventPublisher : ITicketEventPublisher
{
    public Task PublishTicketSubmittedAsync(
        Ticket ticket, 
        TicketSubmittedEvent payload,
        CancellationToken ct = default)
    {
        return Task.CompletedTask; 
    }
}
