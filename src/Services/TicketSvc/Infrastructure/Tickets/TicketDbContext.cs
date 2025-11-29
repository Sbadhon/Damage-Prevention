using Microsoft.EntityFrameworkCore;
using TicketSvc.Domain.Tickets;
using SharedKernel.Tenancy;
using TicketSvc.Domain.Outbox;

namespace TicketSvc.Infrastructure.Tickets;

public sealed class TicketDbContext : DbContext
{
    public TicketDbContext(DbContextOptions<TicketDbContext> options)
        : base(options)
    {
    }

    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var t = modelBuilder.Entity<Ticket>();

        t.ToTable("tickets");

        t.HasKey(x => x.Id);

        t.Property(x => x.Id)
            .ValueGeneratedNever();

        t.Property(x => x.TenantId)
            .HasConversion(
                v => v.Value,       // store as string in DB
                v => new TenantId(v) // read from DB as TenantId
            )
            .IsRequired()
            .HasMaxLength(64);

        t.Property(x => x.WorkType)
            .IsRequired()
            .HasMaxLength(200);

        t.Property(x => x.Address)
            .IsRequired()
            .HasMaxLength(400);

        t.Property(x => x.Description)
            .HasMaxLength(1000);

        t.Property(x => x.Lat)
            .IsRequired();

        t.Property(x => x.Lon)
            .IsRequired();

        t.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        t.Property(x => x.CreatedAt)
            .IsRequired();

        t.Property(x => x.SubmittedAt);
        t.Property(x => x.CompletedAt);
        t.Property(x => x.CancelledAt);
    }
}
