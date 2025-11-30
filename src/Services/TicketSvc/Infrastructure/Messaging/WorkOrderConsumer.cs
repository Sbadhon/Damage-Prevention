using MassTransit;
using Microsoft.Extensions.Logging;
using Contracts.Scheduling;
using TicketSvc.Domain.Abstractions;
using TicketSvc.Domain.Tickets;

public sealed class WorkOrderConsumer :
    IConsumer<WorkOrderAssignedEvent>,
    IConsumer<WorkOrderCompletedEvent>,
    IConsumer<WorkOrderCancelledEvent>
{
    private readonly ILogger<WorkOrderConsumer> _logger;
    private readonly ITicketRepository _repository;

    public WorkOrderConsumer(
        ILogger<WorkOrderConsumer> logger,
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

        ticket.MarkAssigned(evt.CrewId);
        await _repository.SaveChangesAsync();

        _logger.LogInformation(
            "Ticket {TicketId} set to Assigned and assigned to Crew {CrewId} because WorkOrder {WorkOrderId} was assigned",
            evt.TicketId, evt.CrewId, evt.WorkOrderId);
    }

    public async Task Consume(ConsumeContext<WorkOrderCompletedEvent> context)
    {
        var evt = context.Message;

        var ticket = await _repository.GetByIdAsync(evt.TicketId);
        if (ticket == null)
        {
            _logger.LogWarning("Ticket {TicketId} not found for WorkOrderCompletedEvent", evt.TicketId);
            return;
        }

        ticket.Complete(evt.CompletedAt);
        await _repository.SaveChangesAsync();

        _logger.LogInformation(
            "Ticket {TicketId} marked as Completed because WorkOrder {WorkOrderId} was completed",
            evt.TicketId, evt.WorkOrderId);
    }

    public async Task Consume(ConsumeContext<WorkOrderCancelledEvent> context)
    {
        var evt = context.Message;

        var ticket = await _repository.GetByIdAsync(evt.TicketId);
        if (ticket == null)
        {
            _logger.LogWarning("Ticket {TicketId} not found for WorkOrderCancelledEvent", evt.TicketId);
            return;
        }

        // Reopen the ticket (mark as submitted/draft depending on your workflow)
        ticket.Submit(evt.CancelledAt); 
        await _repository.SaveChangesAsync();

        _logger.LogInformation(
            "Ticket {TicketId} reopened because WorkOrder {WorkOrderId} was cancelled",
            evt.TicketId, evt.WorkOrderId);
    }
}
