using System.Text.Json;
using Confluent.Kafka;
using Contracts.Tickets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SchedulingSvc.Domain.Assignments;

namespace SchedulingSvc.Infrastructure;

public class TicketSubmittedConsumer : BackgroundService
{
    private readonly ILogger<TicketSubmittedConsumer> _logger;
    private readonly IServiceProvider _services;
    private readonly string _bootstrapServers;
    private readonly string _topic;

    public TicketSubmittedConsumer(
        ILogger<TicketSubmittedConsumer> logger,
        IServiceProvider services,
        IConfiguration config)
    {
        _logger = logger;
        _services = services;

        var kafkaSection = config.GetSection("Kafka");
        _bootstrapServers = kafkaSection["BootstrapServers"] ?? "localhost:9092";
        _topic = kafkaSection["TicketSubmittedTopic"] ?? "ticket-submitted";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _bootstrapServers,
            GroupId = "scheduling-svc",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
        consumer.Subscribe(_topic);

        _logger.LogInformation("TicketSubmittedConsumer subscribed to topic {Topic}", _topic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                ConsumeResult<string, string>? result;

                try
                {
                    result = consumer.Consume(stoppingToken);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Kafka consume error");
                    continue;
                }

                if (result is null)
                    continue;

                TicketSubmittedEvent? evt = null;

                try
                {
                    evt = JsonSerializer.Deserialize<TicketSubmittedEvent>(result.Message.Value);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to deserialize TicketSubmittedEvent");
                    continue;
                }

                if (evt is null)
                    continue;

                _logger.LogInformation("Received TicketSubmittedEvent for TicketId={TicketId}", evt.TicketId);

                // Handle with a scoped DbContext
                using var scope = _services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<SchedulingDbContext>();

                // Simple assignment logic for now: hard-code region+crew
                var assignment = WorkAssignment.CreateForTicket(
                    evt.TicketId,
                    region: "North-Region",
                    crew: "Crew-Alpha",
                    createdAt: evt.SubmittedAt
                );

                assignment.MarkAssigned();

                db.Assignments.Add(assignment);
                await db.SaveChangesAsync(stoppingToken);

                _logger.LogInformation(
                    "Saved WorkAssignment {AssignmentId} for Ticket {TicketId}",
                    assignment.Id,
                    assignment.TicketId);
            }
        }
        finally
        {
            consumer.Close();
        }
    }

}
