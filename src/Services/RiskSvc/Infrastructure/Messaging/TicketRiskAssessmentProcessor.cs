using Contracts.Tickets;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using RiskSvc.Application.Risk.Commands;

namespace RiskSvc.Infrastructure.Messaging;

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
            "RiskSvc (RabbitMQ) received TicketSubmittedEvent for TicketId={TicketId} Tenant={TenantId}",
            evt.TicketId,
            evt.TenantId);

        var cmd = new RecordRiskAssessmentCommand
        {
            TenantId = evt.TenantId,
            TicketId = evt.TicketId,
            WorkType = evt.WorkType,
            Address = evt.Address,
            Lat = evt.Lat,
            Lon = evt.Lon
        };

        var result = await _sender.Send(cmd, context.CancellationToken);

        _logger.LogInformation(
            "RiskSvc recorded risk assessment for TicketId={TicketId} RiskLevel={Level} Score={Score}",
            result.TicketId,
            result.Level,
            result.Score
        );
    }
}
