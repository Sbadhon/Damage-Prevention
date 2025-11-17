using Xunit;
using SharedKernel;
using Contracts.Tickets;
using TicketSvc.Application.Tickets.Commands;
using TicketSvc.Domain.Abstractions;
using TicketSvc.Domain.Events;
using TicketSvc.Domain.Tickets;

namespace TicketSvc.Tests;

public class SubmitTicketCommandTests
{
    // Fake clock for deterministic tests
    private sealed class FakeClock : IDateTime
    {
        public DateTime FixedUtcNow { get; set; }
        public DateTime UtcNow => FixedUtcNow;
    }

    // In-memory fake repository
    private sealed class FakeTicketRepository : ITicketRepository
    {
        public readonly Dictionary<Guid, Ticket> Store = new();

        public Task AddAsync(Ticket ticket, CancellationToken ct = default)
        {
            Store[ticket.Id] = ticket;
            return Task.CompletedTask;
        }

        public Task<Ticket?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            Store.TryGetValue(id, out var ticket);
            return Task.FromResult(ticket);
        }

        public Task SaveChangesAsync(CancellationToken ct = default)
        {
            // No database — nothing to save.
            return Task.CompletedTask;
        }

        public Task<(IReadOnlyList<Ticket> Items, int TotalCount)> ListByTenantAsync(
            string tenantId,
            int page,
            int pageSize,
            CancellationToken ct = default)
        {
            var all = Store.Values
                .Where(t => t.TenantId == tenantId)
                .ToList();

            var paged = all
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList()
                .AsReadOnly();

            return Task.FromResult<(IReadOnlyList<Ticket>, int)>((paged, all.Count));
        }
    }

    // Fake event publisher
    private sealed class FakeTicketEventPublisher : ITicketEventPublisher
    {
        public readonly List<TicketSubmittedEvent> Published = new();

        public Task PublishTicketSubmittedAsync(
            Ticket ticket,
            TicketSubmittedEvent payload,
            CancellationToken ct = default)
        {
            Published.Add(payload);
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Given_valid_request_When_SubmitTicketCommand_is_handled_Then_ticket_is_submitted()
    {
        // Given
        var clock = new FakeClock
        {
            FixedUtcNow = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc)
        };

        var repo = new FakeTicketRepository();
        var events = new FakeTicketEventPublisher();

        var handler = new SubmitTicketCommandHandler(repo, events, clock);

        var cmd = new SubmitTicketCommand
        {
            TenantId = "tenant-123",
            WorkType = "locate",
            Address = "123 Main",
            Description = "Broken line",
            Lat = 44.95,
            Lon = -93.09
        };

        // When
        var id = await handler.Handle(cmd, CancellationToken.None);

        // Then
        var stored = await repo.GetByIdAsync(id);
        Assert.NotNull(stored);

        Assert.Equal(TicketStatus.Submitted, stored!.Status);
        Assert.Equal("locate", stored.WorkType);
        Assert.Equal("123 Main", stored.Address);
        Assert.Equal("Broken line", stored.Description);

        // Check event was published
        Assert.Single(events.Published);
        var evt = events.Published[0];

        Assert.Equal(id, evt.TicketId);
        Assert.Equal("locate", evt.WorkType);
        Assert.Equal("123 Main", evt.Address);
        Assert.Equal("Broken line", evt.Description);
        Assert.Equal(clock.FixedUtcNow, evt.SubmittedAt);
    }
}
