using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Contracts.Tickets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TicketSvc.Domain.Events;
using TicketSvc.Domain.Tickets;

namespace TicketSvc.Infrastructure.Events;

public sealed class AzureServiceBusTicketEventPublisher : ITicketEventPublisher, IAsyncDisposable
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;
    private readonly ILogger<AzureServiceBusTicketEventPublisher> _logger;

    public AzureServiceBusTicketEventPublisher(
        IConfiguration configuration,
        ILogger<AzureServiceBusTicketEventPublisher> logger)
    {
        _logger = logger;

        var useEmulator = configuration.GetValue<bool>("UseServiceBusEmulator");

        var connectionString = configuration.GetConnectionString("ServiceBus")
            ?? throw new InvalidOperationException("ServiceBus connection string is not configured.");

        var topicName = configuration["TicketEvents:TopicName"] ?? "ticket-submitted";

        // Important difference: For emulator we must disable SSL
        var clientOptions = new ServiceBusClientOptions();

        if (useEmulator)
        {
            clientOptions.TransportType = ServiceBusTransportType.AmqpTcp;
            clientOptions.EnableCrossEntityTransactions = false;

            _logger.LogInformation("Using Azure Service Bus Emulator at {ConnectionString}", connectionString);
        }
        else
        {
            _logger.LogInformation("Using REAL Azure Service Bus");
        }

        _client = new ServiceBusClient(connectionString, clientOptions);
        _sender = _client.CreateSender(topicName);
    }

    public async Task PublishTicketSubmittedAsync(
        Ticket ticket,
        TicketSubmittedEvent payload,
        CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(payload);

        var message = new ServiceBusMessage(json)
        {
            Subject = "TicketSubmitted",
            MessageId = payload.TicketId.ToString()
        };

        try
        {
            await _sender.SendMessageAsync(message, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to publish TicketSubmitted event for TicketId={TicketId}",
                ticket.Id);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _sender.DisposeAsync();
        await _client.DisposeAsync();
    }
}
