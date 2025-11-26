using System.Text.Json;
using Confluent.Kafka;
using Contracts.Tickets;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SchedulingSvc.Application.WorkOrders.Commands;
using SharedKernel.Options;

namespace SchedulingSvc.Infrastructure.Messaging;

/// <summary>
/// Kafka consumer that listens for TicketSubmittedEvent and creates work orders.
/// Uses TicketEvents:Kafka settings from configuration.
/// Will NEVER crash the host; all errors are logged and retried.
/// </summary>
public sealed class TicketSubmittedEventProcessor : BackgroundService
{
    private readonly TicketEventsOptions _options;
    private readonly ILogger<TicketSubmittedEventProcessor> _logger;
    private readonly IServiceProvider _services;

    public TicketSubmittedEventProcessor(
        IOptions<TicketEventsOptions> options,
        ILogger<TicketSubmittedEventProcessor> logger,
        IServiceProvider services)
    {
        _options = options.Value;
        _logger = logger;
        _services = services;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.UseKafka)
        {
            _logger.LogInformation(
                "TicketSubmittedEventProcessor: Kafka not enabled (TicketEvents:UseKafka=false). Processor will be idle.");
            return;
        }

        if (string.IsNullOrWhiteSpace(_options.Kafka.BootstrapServers)
            || string.IsNullOrWhiteSpace(_options.Kafka.TicketSubmittedTopic)
            || string.IsNullOrWhiteSpace(_options.Kafka.GroupId))
        {
            _logger.LogError(
                "TicketSubmittedEventProcessor: Kafka is enabled but configuration is incomplete. " +
                "BootstrapServers={Bootstrap}, Topic={Topic}, GroupId={GroupId}",
                _options.Kafka.BootstrapServers,
                _options.Kafka.TicketSubmittedTopic,
                _options.Kafka.GroupId);

            return;
        }

        var cfg = new ConsumerConfig
        {
            BootstrapServers = _options.Kafka.BootstrapServers,
            GroupId = _options.Kafka.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };

        using var consumer = new ConsumerBuilder<string, string>(cfg).Build();

        consumer.Subscribe(_options.Kafka.TicketSubmittedTopic);

        _logger.LogInformation(
            "SchedulingSvc Kafka consumer started. Topic={Topic}, GroupId={GroupId}, BootstrapServers={Bootstrap}",
            _options.Kafka.TicketSubmittedTopic,
            _options.Kafka.GroupId,
            _options.Kafka.BootstrapServers);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var cr = consumer.Consume(stoppingToken);

                    if (cr == null || string.IsNullOrWhiteSpace(cr.Message.Value))
                    {
                        continue;
                    }

                    TicketSubmittedEvent? evt = null;

                    try
                    {
                        evt = JsonSerializer.Deserialize<TicketSubmittedEvent>(cr.Message.Value);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Failed to deserialize TicketSubmittedEvent from Kafka at offset {Offset}",
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

                    await HandleTicketSubmittedAsync(evt, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // normal shutdown
                    break;
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Kafka consume error in SchedulingSvc.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error in Kafka consumer loop in SchedulingSvc.");
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

    private async Task HandleTicketSubmittedAsync(
        TicketSubmittedEvent evt,
        CancellationToken ct)
    {
        using var scope = _services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var cmd = new CreateWorkOrderCommand
        {
            TenantId = evt.TenantId,
            TicketId = evt.TicketId,
            WorkType = evt.WorkType,
            Address = evt.Address,
            Lat = evt.Lat,
            Lon = evt.Lon
        };

        await sender.Send(cmd, ct);
    }
}
