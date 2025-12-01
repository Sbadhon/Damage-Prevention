using System.Threading.Tasks;
using Contracts.Tickets;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using SchedulingSvc.Application.WorkOrders.Commands;
using SchedulingSvc.Domain.WorkOrders;

namespace SchedulingSvc.Infrastructure.Messaging
{
    public sealed class TicketCancelledConsumer : IConsumer<TicketCancelledEvent>
    {
        private readonly ILogger<TicketCancelledConsumer> _logger;
        private readonly ISender _sender;

        public TicketCancelledConsumer(
            ILogger<TicketCancelledConsumer> logger,
            ISender sender)
        {
            _logger = logger;
            _sender = sender;
        }

        public async Task Consume(ConsumeContext<TicketCancelledEvent> context)
        {
            var evt = context.Message;

            _logger.LogInformation(
                "SchedulingSvc (RabbitMQ) received TicketCancelledEvent for TicketId={TicketId} Tenant={TenantId}",
                evt.TicketId,
                evt.TenantId);

            var updateStatusCmd = new UpdateWorkOrderStatusCommand
            {
                TenantId = evt.TenantId,
                Status = WorkOrderStatus.Cancelled
            };

            // Send the update work order command via MediatR
            await _sender.Send(updateStatusCmd, context.CancellationToken);

            _logger.LogInformation(
                "Cancelled WorkOrder for TicketId={TicketId}",
                evt.TicketId);
        }
    }
}
