using Contracts.Tickets;
using TicketSvc.Domain.Tickets;

namespace TicketSvc.Domain.Events;

public interface ITicketEventPublisher
{
    Task PublishTicketSubmittedAsync(
        Ticket ticket,
        TicketSubmittedEvent payload,
        CancellationToken ct = default);
}
