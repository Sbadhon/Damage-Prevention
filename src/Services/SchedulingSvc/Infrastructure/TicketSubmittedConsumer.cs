using System.Threading.Tasks;
using Contracts.Tickets;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using SchedulingSvc.Application.WorkOrders.Commands;

namespace SchedulingSvc.Infrastructure.Messaging;

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

        var cmd = new CreateWorkOrderCommand
        {
            TenantId = evt.TenantId,
            TicketId = evt.TicketId,
            WorkType = evt.WorkType,
            Address = evt.Address,
            Lat = evt.Lat,
            Lon = evt.Lon
        };

        await _sender.Send(cmd, context.CancellationToken);
    }
}
