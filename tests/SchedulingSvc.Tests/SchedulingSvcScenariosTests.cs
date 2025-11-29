using System;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Moq;
using SchedulingSvc.Application.WorkOrders.Commands;
using SchedulingSvc.Domain.Abstractions;
using SchedulingSvc.Domain.WorkOrders;
using SchedulingSvc.Infrastructure;
using SchedulingSvc.Infrastructure.WorkOrders;
using Contracts.Scheduling;
using SharedKernel;
using Xunit;

namespace SchedulingSvc.Tests
{
    public class SchedulingSvcScenariosTests : IDisposable
    {
        private readonly SchedulingDbContext _db;
        private readonly IWorkOrderRepository _repo;
        private readonly IDateTime _clock;
        private readonly Mock<IPublishEndpoint> _mockPublisher;

        public SchedulingSvcScenariosTests()
        {
            var options = new DbContextOptionsBuilder<SchedulingDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _db = new SchedulingDbContext(options);
            _repo = new EfWorkOrderRepository(_db);
            _clock = new TestDateTime();
            _mockPublisher = new Mock<IPublishEndpoint>();

            // Default mock behavior for generic Publish<T>
            _mockPublisher
                .Setup(x => x.Publish(It.IsAny<WorkOrderAssignedEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        // Scenario: Create WorkOrder
        [Fact]
        public async Task CreateWorkOrderCommand_creates_work_order()
        {
            var cmd = new CreateWorkOrderCommand
            {
                TenantId = "tenant1",
                TicketId = Guid.NewGuid(),
                WorkType = "Repair Cable",
                Address = "123 Main St",
                Lat = 44.9537,
                Lon = -93.09
            };

            var handler = new CreateWorkOrderCommandHandler(_repo, _clock);

            var dto = await handler.Handle(cmd, CancellationToken.None);

            Assert.NotNull(dto);
            Assert.Equal(cmd.TicketId, dto.TicketId);
            Assert.Equal(cmd.Address, dto.Address);
            Assert.Equal(cmd.WorkType, dto.WorkType);

            var workOrder = await _repo.GetByIdAsync(dto.Id);
            Assert.NotNull(workOrder);
            Assert.Equal(WorkOrderStatus.Pending, workOrder!.Status);
        }

        // Scenario: Assign Crew
        [Fact]
        public async Task AssignCrewCommand_assigns_work_order_to_crew()
        {
            // Arrange: create a work order first
            var workOrder = WorkOrder.CreateFromTicket(
                "tenant1",
                Guid.NewGuid(),
                "Repair Cable",
                "123 Main St",
                44.9537,
                -93.09,
                _clock.UtcNow
            );

            await _repo.AddAsync(workOrder);
            await _repo.SaveChangesAsync();

            var cmd = new AssignCrewCommand
            {
                TenantId = "tenant1",
                WorkOrderId = workOrder.Id,
                CrewId = "crew1",
                CrewName = "Alpha Team"
            };

            var handler = new AssignCrewCommandHandler(_repo, _clock, _mockPublisher.Object);

            // Act
            await handler.Handle(cmd, CancellationToken.None);

            // Assert
            var fetched = await _repo.GetByIdAsync(workOrder.Id);
            Assert.NotNull(fetched);
            Assert.Equal(WorkOrderStatus.Assigned, fetched!.Status);
            Assert.Equal(cmd.CrewId, fetched.CrewId);
            Assert.Equal(cmd.CrewName, fetched.CrewName);

            // Verify the generic Publish<T> call
            _mockPublisher.Verify(x =>
                x.Publish(It.IsAny<WorkOrderAssignedEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // Scenario: Complete WorkOrder
        [Fact]
        public async Task UpdateWorkOrderStatusCommand_completes_work_order()
        {
            var workOrder = WorkOrder.CreateFromTicket(
                "tenant1",
                Guid.NewGuid(),
                "Repair Cable",
                "123 Main St",
                44.9537,
                -93.09,
                _clock.UtcNow
            );
            workOrder.AssignCrew("crew1", "Alpha Team", _clock.UtcNow);

            await _repo.AddAsync(workOrder);
            await _repo.SaveChangesAsync();

            var cmd = new UpdateWorkOrderStatusCommand
            {
                TenantId = "tenant1",
                WorkOrderId = workOrder.Id,
                Status = WorkOrderStatus.Completed
            };

            var handler = new UpdateWorkOrderStatusCommandHandler(_repo, _clock);

            await handler.Handle(cmd, CancellationToken.None);

            var fetched = await _repo.GetByIdAsync(workOrder.Id);
            Assert.NotNull(fetched);
            Assert.Equal(WorkOrderStatus.Completed, fetched!.Status);
            Assert.NotNull(fetched.CompletedAt);
        }

        public void Dispose() => _db.Dispose();

        // Test IDateTime implementation
        private class TestDateTime : IDateTime
        {
            public DateTime UtcNow => DateTime.UtcNow;
        }
    }
}
