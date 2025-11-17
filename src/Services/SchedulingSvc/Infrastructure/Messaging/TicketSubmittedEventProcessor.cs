using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Confluent.Kafka;
using Contracts.Tickets;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel.Options;
using SchedulingSvc.Application.WorkOrders.Commands;

namespace SchedulingSvc.Infrastructure.Messaging;

public sealed class TicketSubmittedEventProcessor : BackgroundService
{
    private readonly TicketEventsOptions _options;
    private readonly ILogger<TicketSubmittedEventProcessor> _logger;
    private readonly IServiceProvider _services;

    private ServiceBusClient? _client;
    private ServiceBusProcessor? _processor;

    private readonly bool _messagingEnabled = true;

    public TicketSubmittedEventProcessor(
        IOptions<TicketEventsOptions> options,
        ILogger<TicketSubmittedEventProcessor> logger,
        IServiceProvider services)
    {
        _options  = options.Value;
        _logger   = logger;
        _services = services;

        try
        {
            if (_options.UseAzureServiceBus)
            {
                if (string.IsNullOrWhiteSpace(_options.AzureServiceBus.ConnectionString))
                {
                    throw new InvalidOperationException(
                        "ServiceBus connection string is not configured (TicketEvents:AzureServiceBus:ConnectionString).");
                }

                var topicName    = _options.AzureServiceBus.TopicName;
                var subscription = _options.AzureServiceBus.SubscriptionName;

                _client = new ServiceBusClient(_options.AzureServiceBus.ConnectionString);
                _processor = _client.CreateProcessor(topicName, subscription, new ServiceBusProcessorOptions());

                _logger.LogInformation(
                    "SchedulingSvc configured to use Azure Service Bus. Topic={Topic}, Subscription={Subscription}",
                    topicName,
                    subscription);
            }

            if (_options.UseKafka)
            {
                if (string.IsNullOrWhiteSpace(_options.Kafka.BootstrapServers))
                {
                    _logger.LogWarning(
                        "Kafka is enabled but BootstrapServers is empty. SchedulingSvc consumer will not be able to connect.");
                }
                else
                {
                    _logger.LogInformation(
                        "SchedulingSvc configured to use Kafka. BootstrapServers={Servers}, Topic={Topic}, GroupId={GroupId}",
                        _options.Kafka.BootstrapServers,
                        _options.Kafka.TicketSubmittedTopic,
                        _options.Kafka.GroupId);
                }
            }

            if (!_options.UseAzureServiceBus && !_options.UseKafka)
            {
                _logger.LogInformation(
                    "Neither Azure Service Bus nor Kafka is enabled; TicketSubmittedEventProcessor will be idle.");
                _messagingEnabled = false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to initialize TicketSubmittedEventProcessor. Messaging will be disabled, but API will stay up.");
            _messagingEnabled = false;
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_messagingEnabled)
        {
            _logger.LogWarning(
                "TicketSubmittedEventProcessor messaging disabled; background processing loop will not start.");
            return;
        }

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

    // ===== Azure Service Bus path =====

    private async Task RunAzureServiceBusLoop(CancellationToken stoppingToken)
    {
        if (_processor is null)
        {
            _logger.LogWarning(
                "Azure Service Bus is enabled but ServiceBusProcessor is null; skipping ASB loop.");
            return;
        }

        _processor.ProcessMessageAsync += OnAsbMessageAsync;
        _processor.ProcessErrorAsync   += OnAsbErrorAsync;

        _logger.LogInformation(
            "Starting TicketSubmittedEventProcessor on Azure Service Bus (topic: {Topic}, subscription: {Subscription})",
            _options.AzureServiceBus.TopicName,
            _options.AzureServiceBus.SubscriptionName);

        await _processor.StartProcessingAsync(stoppingToken);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown
        }
    }

    private async Task OnAsbMessageAsync(ProcessMessageEventArgs args)
    {
        try
        {
            var body = args.Message.Body.ToString();
            var evt  = JsonSerializer.Deserialize<TicketSubmittedEvent>(body);

            if (evt is null)
            {
                _logger.LogWarning("Received null/invalid TicketSubmittedEvent payload via ASB");
                await args.CompleteMessageAsync(args.Message);
                return;
            }

            await HandleTicketEventWithRetryAsync(evt, args.CancellationToken);
            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling TicketSubmittedEvent in SchedulingSvc via ASB");
        }
    }

    private Task OnAsbErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(
            args.Exception,
            "Service Bus processing error in SchedulingSvc: {ErrorSource}",
            args.ErrorSource);

        return Task.CompletedTask;
    }

    // ===== Kafka path =====

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
                    var cr   = consumer.Consume(ct);
                    var json = cr.Message.Value;

                    TicketSubmittedEvent? evt = null;

                    try
                    {
                        evt = JsonSerializer.Deserialize<TicketSubmittedEvent>(json);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
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

                    await HandleTicketEventWithRetryAsync(evt, ct);
                }
                catch (OperationCanceledException)
                {
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

    // ===== Shared handler logic + retry =====

    private async Task HandleTicketEventWithRetryAsync(TicketSubmittedEvent evt, CancellationToken ct)
    {
        await ExecuteWithRetryAsync(
            operationName: $"ScheduleWorkOrder for TicketId={evt.TicketId}",
            action: () => HandleTicketEventAsync(evt, ct),
            ct: ct);
    }

    private async Task HandleTicketEventAsync(TicketSubmittedEvent evt, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(evt.TenantId))
        {
            _logger.LogWarning(
                "Skipping TicketSubmittedEvent with missing TenantId in SchedulingSvc. TicketId={TicketId}",
                evt.TicketId);
            return;
        }

        using var scope = _services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        // NOTE: adjust command name to match your actual implementation
        var cmd = new CreateWorkOrderCommand
        {
            TenantId = evt.TenantId,
            TicketId = evt.TicketId,
            WorkType = evt.WorkType,
            Address  = evt.Address,
            Lat      = evt.Lat,
            Lon      = evt.Lon
        };

        await sender.Send(cmd, ct);
    }

    private async Task ExecuteWithRetryAsync(string operationName, Func<Task> action, CancellationToken ct)
    {
        const int maxAttempts = 3;
        var delay = TimeSpan.FromSeconds(1);

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await action();
                return;
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                if (attempt == maxAttempts)
                {
                    _logger.LogError(ex,
                        "Operation {Operation} failed after {Attempts} attempts in SchedulingSvc.",
                        operationName,
                        attempt);
                    throw;
                }

                _logger.LogWarning(ex,
                    "Operation {Operation} failed on attempt {Attempt}/{MaxAttempts} in SchedulingSvc. Retrying in {Delay}...",
                    operationName,
                    attempt,
                    maxAttempts,
                    delay);

                try
                {
                    await Task.Delay(delay, ct);
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    throw;
                }

                delay = TimeSpan.FromSeconds(delay.TotalSeconds * 2);
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_processor is not null)
        {
            await _processor.StopProcessingAsync(cancellationToken);
            await _processor.DisposeAsync();
        }

        if (_client is not null)
        {
            await _client.DisposeAsync();
        }

        await base.StopAsync(cancellationToken);
    }
}
