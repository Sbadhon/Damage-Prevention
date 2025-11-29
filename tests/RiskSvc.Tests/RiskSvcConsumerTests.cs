using System;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Tickets;
using MassTransit;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using RiskSvc.Application.Risk.Commands;
using RiskSvc.Domain.Abstractions;
using RiskSvc.Domain.Risk;
using RiskSvc.Infrastructure.Risk;
using RiskSvc.Infrastructure.Messaging;
using SharedKernel;
using Xunit;

namespace RiskSvc.Tests;

public class RiskSvcConsumerTests : IAsyncLifetime
{
    private readonly ITestHarness _harness;
    private readonly RiskDbContext _db;
    private readonly IRiskAssessmentRepository _repo;
    private readonly IDateTime _clock;

    public RiskSvcConsumerTests()
    {
        // In-memory EF Core
        var options = new DbContextOptionsBuilder<RiskDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new RiskDbContext(options);
        _repo = new EfCoreRiskAssessmentRepository(_db);
        _clock = new TestDateTime();

        // MassTransit test harness with consumer
        _harness = new InMemoryTestHarness();
        
        _harness.OnConfigureInMemoryBus += cfg =>
        {
            cfg.AddConsumer(() => new TicketSubmittedConsumer(_repo, _clock));
        };
    }

    // Start/Stop MassTransit
    public async Task InitializeAsync() => await _harness.Start();
    public async Task DisposeAsync() => await _harness.Stop();

    // Scenario: TicketSubmittedEvent triggers risk assessment creation
    [Fact]
    public async Task TicketSubmittedEvent_creates_risk_assessment()
    {
        var evt = new TicketSubmittedEvent(
            TicketId: Guid.NewGuid(),
            TenantId: "tenant1",
            WorkType: "Locate fiber",
            Address: "123 Main St",
            Lat: 44.95,
            Lon: -93.09,
            SubmittedAt: _clock.UtcNow
        );

        await _harness.InputQueueSendEndpoint.Send(evt);

        Assert.True(await _harness.Consumed.Any<TicketSubmittedEvent>(), 
            "Event was not consumed by the consumer.");

        // Verify risk was created
        var stored = _db.RiskAssessments.FirstOrDefault(r => r.TicketId == evt.TicketId);
        Assert.NotNull(stored);

        Assert.Equal(evt.TenantId, stored!.TenantId);
        Assert.Equal(evt.WorkType, stored.WorkType);
        Assert.Equal(evt.Address, stored.Address);
        Assert.Equal(evt.Lat, stored.Lat);
        Assert.Equal(evt.Lon, stored.Lon);
    }

    // Scenario: Tenant isolation on event consumer
    [Fact]
    public async Task TicketSubmittedEvent_respects_tenant_id_on_created_risk()
    {
        var evt = new TicketSubmittedEvent(
            TicketId: Guid.NewGuid(),
            TenantId: "tenantA",
            WorkType: "fiber",
            Address: "111",
            Lat: 1,
            Lon: 1,
            SubmittedAt: _clock.UtcNow
        );

        await _harness.InputQueueSendEndpoint.Send(evt);

        var stored = _db.RiskAssessments.FirstOrDefault(r => r.TicketId == evt.TicketId);
        Assert.NotNull(stored);
        Assert.Equal("tenantA", stored!.TenantId);
    }

    // Scenario: Risk is computed from work type
    [Fact]
    public async Task TicketSubmittedEvent_computes_score_from_work_type()
    {
        var evt = new TicketSubmittedEvent(
            TicketId: Guid.NewGuid(),
            TenantId: "tenant1",
            WorkType: "blast",   // should be High risk
            Address: "XYZ",
            Lat: 0,
            Lon: 0,
            SubmittedAt: _clock.UtcNow
        );

        await _harness.InputQueueSendEndpoint.Send(evt);

        var stored = _db.RiskAssessments.FirstOrDefault(r => r.TicketId == evt.TicketId);
        Assert.NotNull(stored);
        Assert.Equal("High", stored!.Level);
        Assert.True(stored.Score >= 0.8);
    }

    private class TestDateTime : IDateTime
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}
