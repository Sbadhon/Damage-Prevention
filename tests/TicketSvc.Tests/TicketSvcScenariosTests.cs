using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Tickets;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using SharedKernel.Tenancy;
using TicketSvc.Application.Common.Tenancy;
using TicketSvc.Application.Tickets.Commands;
using TicketSvc.Domain.Abstractions;
using TicketSvc.Domain.Tickets;
using TicketSvc.Infrastructure.Tickets;
using TicketSvc.Domain.Outbox;
using Xunit;
using TicketSvc.Infrastructure.Outbox;

namespace TicketSvc.Tests;

public class TicketSvcScenariosTests : IDisposable
{
    private readonly TicketDbContext _db;
    private readonly ITicketRepository _repo;
    private readonly IOutboxRepository _outbox;
    private readonly IDateTime _clock;

    public TicketSvcScenariosTests()
    {
        var options = new DbContextOptionsBuilder<TicketDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new TicketDbContext(options);
        _repo = new EfTicketRepository(_db);
        _outbox = new EfOutboxRepository(_db);
        _clock = new TestDateTime();
    }

    // Scenario: Submit Ticket
    [Fact]
    public async Task SubmitTicketCommand_creates_ticket_and_submits_it()
    {
        var cmd = new SubmitTicketCommand
        {
            TenantId = "tenant1",
            WorkType = "Repair Cable",
            Address = "123 Main St",
            Description = "Fiber repair",
            Lat = 44.9537,
            Lon = -93.09
        };

        var handler = new SubmitTicketCommandHandler(_repo, _outbox, _clock);

        var ticketId = await handler.Handle(cmd, CancellationToken.None);

        var ticket = await _repo.GetByIdAsync(ticketId);
        Assert.NotNull(ticket);
        Assert.Equal(TicketStatus.Submitted, ticket!.Status);
        Assert.Equal(cmd.Address, ticket.Address);
        Assert.Equal(cmd.WorkType, ticket.WorkType);
        Assert.Equal(cmd.TenantId, ticket.TenantId.Value);

        // Instead of calling GetByTicketIdAsync (not on interface), query DbContext directly
        var outboxMessage = await _db.OutboxMessages
            .Where(m => m.TicketId == ticketId)
            .FirstOrDefaultAsync();

        Assert.NotNull(outboxMessage);
        Assert.Equal(nameof(TicketSubmittedEvent), outboxMessage!.Type);
    }

    // Scenario: Complete Ticket
    [Fact]
    public async Task CompleteTicketCommand_marks_ticket_as_completed()
    {
        var tenantId = "tenant1";
        var ticket = Ticket.CreateDraft(
            new TenantId(tenantId),
            "Repair Cable",
            "123 Main St",
            "Fiber repair",
            0, 0,
            _clock.UtcNow
        );
        ticket.Submit(_clock.UtcNow);

        await _repo.AddAsync(ticket);
        await _repo.SaveChangesAsync();

        var cmd = new CompleteTicketCommand
        {
            TenantId = tenantId,
            TicketId = ticket.Id
        };

        var handler = new CompleteTicketCommandHandler(_repo, _clock);

        await handler.Handle(cmd, CancellationToken.None);

        var fetched = await _repo.GetByIdAsync(ticket.Id);
        Assert.Equal(TicketStatus.Completed, fetched!.Status);
        Assert.NotNull(fetched.CompletedAt);
    }

    // Scenario: Cancel Ticket
    [Fact]
    public async Task CancelTicketCommand_marks_ticket_as_cancelled()
    {
        var tenantId = "tenant1";
        var ticket = Ticket.CreateDraft(
            new TenantId(tenantId),
            "Repair Cable",
            "123 Main St",
            "Fiber repair",
            0, 0,
            _clock.UtcNow
        );
        ticket.Submit(_clock.UtcNow);

        await _repo.AddAsync(ticket);
        await _repo.SaveChangesAsync();

        var cmd = new CancelTicketCommand
        {
            TenantId = tenantId,
            TicketId = ticket.Id
        };

        var handler = new CancelTicketCommandHandler(_repo, _clock);

        await handler.Handle(cmd, CancellationToken.None);

        var fetched = await _repo.GetByIdAsync(ticket.Id);
        Assert.Equal(TicketStatus.Cancelled, fetched!.Status);
        Assert.NotNull(fetched.CancelledAt);
    }

    public void Dispose() => _db.Dispose();

    private class TestDateTime : IDateTime
    {
        public DateTime UtcNow => DateTime.UtcNow; // matches IDateTime interface
    }
}
