using Microsoft.EntityFrameworkCore;
using SchedulingSvc.Domain.WorkOrders;

namespace SchedulingSvc.Infrastructure;

public class SchedulingDbContext : DbContext
{
    public SchedulingDbContext(DbContextOptions<SchedulingDbContext> options)
        : base(options)
    {
    }

    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var wo = modelBuilder.Entity<WorkOrder>();

        wo.ToTable("WorkOrders");

        wo.HasKey(w => w.Id);

        wo.Property(w => w.Id)
            .ValueGeneratedNever();

        wo.Property(w => w.TenantId)
            .IsRequired()
            .HasMaxLength(64);

        wo.Property(w => w.TicketId)
            .IsRequired();

        wo.Property(w => w.WorkType)
            .IsRequired()
            .HasMaxLength(200);

        wo.Property(w => w.Address)
            .IsRequired()
            .HasMaxLength(400);

        wo.Property(w => w.Lat);
        wo.Property(w => w.Lon);

        wo.Property(w => w.CrewId)
            .HasMaxLength(64);

        wo.Property(w => w.CrewName)
            .HasMaxLength(200);

        wo.Property(w => w.Status)
            .HasConversion<int>()
            .IsRequired();

        wo.Property(w => w.CreatedAt)
            .IsRequired();

        wo.Property(w => w.AssignedAt);
        wo.Property(w => w.CompletedAt);
        wo.Property(w => w.CancelledAt);
    }
}
