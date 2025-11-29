using MassTransit;
using Microsoft.Extensions.Logging;
using Contracts.Scheduling;
using TicketSvc.Domain.Abstractions;
using TicketSvc.Domain.Tickets;

public sealed class WorkOrderAssignedConsumer : IConsumer<WorkOrderAssignedEvent>
{
    private readonly ILogger<WorkOrderAssignedConsumer> _logger;
    private readonly ITicketRepository _repository;

    public WorkOrderAssignedConsumer(
        ILogger<WorkOrderAssignedConsumer> logger,
        ITicketRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    public async Task Consume(ConsumeContext<WorkOrderAssignedEvent> context)
    {
        var evt = context.Message;

        var ticket = await _repository.GetByIdAsync(evt.TicketId);

        if (ticket == null)
        {
            _logger.LogWarning("Ticket {TicketId} not found for WorkOrderAssignedEvent", evt.TicketId);
            return;
        }

        ticket.MarkAssigned(); 
        await _repository.SaveChangesAsync();

        _logger.LogInformation(
            "Ticket {TicketId} set to InProgress because WorkOrder {WorkOrderId} was assigned",
            evt.TicketId, evt.WorkOrderId);
    }
}
