using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using IndustrialSecureApi.Infrastructure;
using IndustrialSecureApi.Features.Sensors;

namespace IndustrialSecureApi.Infrastructure;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public string? CurrentUserId { get; set; }

    // DbSet per le entità
    public DbSet<SensorReading> SensorReadings => Set<SensorReading>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurazione SensorReading
        modelBuilder.Entity<SensorReading>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Tag).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Tag); // Indice su Tag come richiesto
        });

        // Configurazione AuditEntry
        modelBuilder.Entity<AuditEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.User).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Entity).IsRequired().HasMaxLength(100);
            entity.Property(e => e.OldValues).HasColumnType("text");
            entity.Property(e => e.NewValues).HasColumnType("text");
            entity.HasIndex(e => e.When);
            entity.HasIndex(e => e.User);
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Soft Delete: marca come eliminati invece di rimuovere
        var deletedEntries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Deleted)
            .ToList();

        foreach (var entry in deletedEntries)
        {
            // Completeremo quando avremo i modelli con proprietà IsDeleted o DeletedAt
            // Per ora lasciamo vuoto
        }

        // Audit Trail: traccia modifiche
        var modifiedEntries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added ||
                       e.State == EntityState.Modified ||
                       e.State == EntityState.Deleted)
            .ToList();

        // Completeremo quando avremo AuditEntry completamente configurato
        // Per ora lasciamo vuoto

        return await base.SaveChangesAsync(cancellationToken);
    }
}