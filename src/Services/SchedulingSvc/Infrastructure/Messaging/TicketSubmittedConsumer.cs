using System.Threading.Tasks;
using Contracts.Tickets;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using SchedulingSvc.Application.Crew;
using SchedulingSvc.Application.WorkOrders.Commands;

namespace SchedulingSvc.Infrastructure.Messaging;

/// <summary>
/// RabbitMQ consumer for TicketSubmittedEvent.
/// Automatically picks a crew based on WorkType and creates a WorkOrder.
/// </summary>
public sealed class TicketSubmittedConsumer : IConsumer<TicketSubmittedEvent>
{
    private readonly ILogger<TicketSubmittedConsumer> _logger;
    private readonly ISender _sender;

    public TicketSubmittedConsumer(
        ILogger<TicketSubmittedConsumer> logger,
        ISender sender)
    {
        _logger = logger;
        _sender = sender;
    }

    public async Task Consume(ConsumeContext<TicketSubmittedEvent> context)
    {
        var evt = context.Message;

        _logger.LogInformation(
            "SchedulingSvc (RabbitMQ) received TicketSubmittedEvent for TicketId={TicketId} Tenant={TenantId}",
            evt.TicketId,
            evt.TenantId);

        // Create the work order from the ticket
        var createCmd = new CreateWorkOrderCommand
        {
            TenantId = evt.TenantId,
            TicketId = evt.TicketId,
            WorkType = evt.WorkType,
            Address  = evt.Address,
            Lat      = evt.Lat,
            Lon      = evt.Lon
        };

        // Send create command via MediatR
        var workOrder = await _sender.Send(createCmd, context.CancellationToken);

        _logger.LogInformation(
            "Created WorkOrder {WorkOrderId} for TicketId={TicketId}",
            workOrder.Id,
            evt.TicketId);
    }
}
