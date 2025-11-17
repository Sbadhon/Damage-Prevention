using System.Text.Json;
using Confluent.Kafka;
using Contracts.Tickets;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel.Options;

namespace SchedulingSvc.Infrastructure.Messaging;

public sealed class TicketSubmittedEventProcessor : BackgroundService
{
    private readonly ILogger<TicketSubmittedEventProcessor> _logger;
    private readonly IServiceProvider _services;
    private readonly TicketEventsOptions _options;

    public TicketSubmittedEventProcessor(
        IOptions<TicketEventsOptions> options,
        ILogger<TicketSubmittedEventProcessor> logger,
        IServiceProvider services)
    {
        _options = options.Value;
        _logger  = logger;
        _services = services;

        // Validate only what's actually enabled
        if (_options.UseAzureServiceBus &&
            string.IsNullOrWhiteSpace(_options.AzureServiceBus.ConnectionString))
        {
            throw new InvalidOperationException(
                "ServiceBus connection string is not configured (TicketEvents:AzureServiceBus:ConnectionString).");
        }

        if (_options.UseKafka &&
            string.IsNullOrWhiteSpace(_options.Kafka.BootstrapServers))
        {
            _logger.LogWarning(
                "Kafka is enabled but TicketEvents:Kafka:BootstrapServers is empty. Events may not be processed.");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_options.UseAzureServiceBus)
        {
            await RunAzureServiceBusLoop(stoppingToken);
        }
        else if (_options.UseKafka)
        {
            await RunKafkaLoop(stoppingToken);
        }
        else
        {
            _logger.LogInformation(
                "No messaging transport configured; TicketSubmittedEventProcessor is idle.");
        }
    }

    private Task RunAzureServiceBusLoop(CancellationToken ct)
    {
        _logger.LogInformation("Azure Service Bus loop for SchedulingSvc not implemented yet.");
        return Task.CompletedTask;
    }

    private async Task RunKafkaLoop(CancellationToken ct)
    {
        var cfg = new ConsumerConfig
        {
            BootstrapServers = _options.Kafka.BootstrapServers,
            GroupId          = _options.Kafka.GroupId,
            AutoOffsetReset  = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };

        using var consumer = new ConsumerBuilder<string, string>(cfg).Build();

        consumer.Subscribe(_options.Kafka.TicketSubmittedTopic);

        _logger.LogInformation(
            "SchedulingSvc Kafka consumer started. Topic={Topic}, GroupId={GroupId}",
            _options.Kafka.TicketSubmittedTopic,
            _options.Kafka.GroupId);

        try
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var cr = consumer.Consume(ct);
                    var json = cr.Message.Value;

                    TicketSubmittedEvent? evt = null;

                    try
                    {
                        evt = JsonSerializer.Deserialize<TicketSubmittedEvent>(json);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Failed to deserialize TicketSubmittedEvent from Kafka message at offset {Offset}",
                            cr.Offset);
                    }

                    if (evt is null)
                    {
                        _logger.LogWarning(
                            "Skipping null/invalid TicketSubmittedEvent from Kafka at offset {Offset}",
                            cr.Offset);
                        continue;
                    }

                    _logger.LogInformation(
                        "SchedulingSvc received TicketSubmittedEvent for TicketId={TicketId} Tenant={TenantId}",
                        evt.TicketId,
                        evt.TenantId);

                    // 🔹 Here you would typically create/schedule a work order
                    using var scope = _services.CreateScope();
                    var sender = scope.ServiceProvider.GetRequiredService<ISender>();

                    // e.g.:
                    // await sender.Send(new CreateWorkOrderFromTicketCommand { ... }, ct);

                    // For now we're just logging, so no command required.
                }
                catch (OperationCanceledException)
                {
                    // Graceful shutdown
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error consuming from Kafka in SchedulingSvc");
                }
            }
        }
        finally
        {
            consumer.Close();
            _logger.LogInformation("SchedulingSvc Kafka consumer stopped.");
        }

        await Task.CompletedTask;
    }
}
