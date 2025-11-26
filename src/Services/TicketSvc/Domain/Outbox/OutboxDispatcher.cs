// using System.Text.Json;
// using Microsoft.Extensions.Hosting;
// using Microsoft.Extensions.Logging;
// using SharedKernel;
// using TicketSvc.Domain.Abstractions;
// using TicketSvc.Domain.Outbox;
// using TicketSvc.Domain.Tickets;
// using TicketSvc.Domain;
// using Contracts.Tickets;
// using TicketSvc.Domain.Events;

// namespace TicketSvc.Infrastructure.Outbox;

// public sealed class OutboxDispatcher : BackgroundService
// {
//     private readonly IOutboxRepository _outbox;
//     private readonly ITicketRepository _tickets;
//     private readonly ITicketEventPublisher _publisher;
//     private readonly IDateTime _clock;
//     private readonly ILogger<OutboxDispatcher> _logger;

//     public OutboxDispatcher(
//         IOutboxRepository outbox,
//         ITicketRepository tickets,
//         ITicketEventPublisher publisher,
//         IDateTime clock,
//         ILogger<OutboxDispatcher> logger)
//     {
//         _outbox = outbox;
//         _tickets = tickets;
//         _publisher = publisher;
//         _clock = clock;
//         _logger = logger;
//     }

//     protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//     {
//         _logger.LogInformation("OutboxDispatcher started.");

//         while (!stoppingToken.IsCancellationRequested)
//         {
//             try
//             {
//                 var batch = await _outbox.GetPendingBatchAsync(20, stoppingToken);

//                 if (batch.Count == 0)
//                 {
//                     await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
//                     continue;
//                 }

//                 foreach (var message in batch)
//                 {
//                     if (stoppingToken.IsCancellationRequested)
//                         break;

//                     try
//                     {
//                         message.MarkProcessing();
//                         await _outbox.UpdateAsync(message, stoppingToken);

//                         await DispatchMessageAsync(message, stoppingToken);

//                         message.MarkPublished(_clock.UtcNow);
//                         await _outbox.UpdateAsync(message, stoppingToken);
//                     }
//                     catch (Exception ex)
//                     {
//                         _logger.LogError(ex,
//                             "Failed to dispatch OutboxMessage {OutboxId} (Type={Type})",
//                             message.Id,
//                             message.Type);

//                         message.MarkFailed(ex.Message, _clock.UtcNow);
//                         await _outbox.UpdateAsync(message, stoppingToken);
//                     }
//                 }
//             }
//             catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
//             {
//                 // normal shutdown
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Unexpected error in OutboxDispatcher loop. Will retry.");
//                 await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
//             }
//         }

//         _logger.LogInformation("OutboxDispatcher stopping.");
//     }

//     private async Task DispatchMessageAsync(OutboxMessage message, CancellationToken ct)
//     {
//         switch (message.Type)
//         {
//             case nameof(TicketSubmittedEvent):
//             {
//                 var evt = JsonSerializer.Deserialize<TicketSubmittedEvent>(message.Payload);

//                 if (evt is null)
//                 {
//                     throw new InvalidOperationException(
//                         $"Failed to deserialize TicketSubmittedEvent for OutboxMessage {message.Id}");
//                 }

//                 // Load the ticket so we can satisfy ITicketEventPublisher signature
//                 var ticket = await _tickets.GetByIdAsync(evt.TicketId, ct);
//                 if (ticket is null)
//                 {
//                     _logger.LogWarning(
//                         "Ticket not found for OutboxMessage {OutboxId}, TicketId={TicketId}. Skipping.",
//                         message.Id,
//                         evt.TicketId);
//                     return;
//                 }

//                 await _publisher.PublishTicketSubmittedAsync(ticket, evt, ct);
//                 break;
//             }

//             default:
//                 _logger.LogWarning(
//                     "Unknown OutboxMessage type {Type} for OutboxMessage {Id}. Skipping.",
//                     message.Type,
//                     message.Id);
//                 break;
//         }
//     }
// }