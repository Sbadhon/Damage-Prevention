using System.Text.Json;
using Confluent.Kafka;
using Contracts.Tickets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TicketSvc.Domain.Events;
using TicketSvc.Domain.Tickets;

namespace TicketSvc.Infrastructure.Events;

public sealed class KafkaTicketEventPublisher : ITicketEventPublisher, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly string _topic;
    private readonly ILogger<KafkaTicketEventPublisher> _logger;

    public KafkaTicketEventPublisher(
        IConfiguration configuration,
        ILogger<KafkaTicketEventPublisher> logger)
    {
        _logger = logger;

        // Read from TicketEvents:Kafka section, consistent with TicketEventsOptions
        var bootstrapServers = configuration["TicketEvents:Kafka:BootstrapServers"]
            ?? throw new InvalidOperationException("TicketEvents:Kafka:BootstrapServers is not configured.");

        _topic = configuration["TicketEvents:Kafka:TopicName"] ?? "ticket-submitted";

        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            Acks = Acks.All
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishTicketSubmittedAsync(
        Ticket ticket,
        TicketSubmittedEvent payload,
        CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(payload);
        var key  = payload.TicketId.ToString();

        try
        {
            var result = await _producer.ProduceAsync(_topic, new Message<string, string>
            {
                Key   = key,
                Value = json
            }, ct);

            _logger.LogInformation(
                "Published TicketSubmitted event to Kafka topic {Topic} with offset {Offset} for TicketId={TicketId}",
                _topic,
                result.Offset,
                ticket.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to publish TicketSubmitted event to Kafka for TicketId={TicketId}",
                ticket.Id);
        }
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
    }
}
