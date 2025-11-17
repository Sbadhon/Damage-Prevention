using Microsoft.EntityFrameworkCore;
using SchedulingSvc.Domain.Assignments;

namespace SchedulingSvc.Infrastructure;

public class SchedulingDbContext : DbContext
{
    public SchedulingDbContext(DbContextOptions<SchedulingDbContext> options)
        : base(options)
    {
    }

    public DbSet<WorkAssignment> Assignments => Set<WorkAssignment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var assignment = modelBuilder.Entity<WorkAssignment>();

        assignment.ToTable("WorkAssignments");

        assignment.HasKey(a => a.Id);

        assignment.Property(a => a.TicketId).IsRequired();
        assignment.Property(a => a.Region).HasMaxLength(100);
        assignment.Property(a => a.Crew).HasMaxLength(100);
        assignment.Property(a => a.Status).HasConversion<int>();
        assignment.Property(a => a.CreatedAt);
    }
}
