using Microsoft.EntityFrameworkCore;
using RiskSvc.Domain.Risk;

namespace RiskSvc.Infrastructure.Risk;

public sealed class RiskDbContext : DbContext
{
    public RiskDbContext(DbContextOptions<RiskDbContext> options)
        : base(options)
    {
    }

    public DbSet<RiskAssessment> RiskAssessments => Set<RiskAssessment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<RiskAssessment>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TenantId)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(e => e.WorkType).IsRequired();
            entity.Property(e => e.Address).IsRequired();
            entity.Property(e => e.Lat).IsRequired();
            entity.Property(e => e.Lon).IsRequired();
            entity.Property(e => e.Score).IsRequired();
            entity.Property(e => e.Level).IsRequired();
            entity.Property(e => e.AssessedAt).IsRequired();

            entity.HasIndex(e => new { e.TenantId, e.TicketId })
                  .IsUnique();
        });
    }
}
