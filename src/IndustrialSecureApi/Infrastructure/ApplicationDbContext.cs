using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using IndustrialSecureApi.Infrastructure;
using IndustrialSecureApi.Features.Sensors;
using System.Text.Json;

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
        // ============================================
        // BLOCCO 1: SOFT DELETE
        // ============================================
        // Trova tutte le entità che stanno per essere eliminate
        var deletedEntries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Deleted)
            .ToList();

        foreach (var entry in deletedEntries)
        {
            // Per ora lasciamo vuoto perché SensorReading è un record immutabile
            // In futuro, se convertiamo in class, potremmo fare:
            // entry.Property("IsDeleted").CurrentValue = true;
            // entry.State = EntityState.Modified;
        }

        // ============================================
        // BLOCCO 2: AUDIT TRAIL - Prepara le entry da tracciare
        // ============================================
        // Trova tutte le entità modificate (aggiunte, modificate, eliminate)
        var modifiedEntries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added ||
                       e.State == EntityState.Modified ||
                       e.State == EntityState.Deleted)
            .ToList();

        // Lista per raccogliere gli AuditEntry da creare
        var auditEntries = new List<AuditEntry>();

        // ============================================
        // BLOCCO 3: AUDIT TRAIL - Itera su ogni entry modificata
        // ============================================
        foreach (var entry in modifiedEntries)
        {
            // Salta se è già un AuditEntry (evita loop infiniti)
            if (entry.Entity is AuditEntry)
            {
                continue;
            }

            // Salta le entità Identity (AspNetUsers, AspNetRoles, etc.)
            // per evitare di loggare ogni cambio password, ruolo, etc.
            var entityType = entry.Entity.GetType();
            if (entityType.Namespace?.Contains("Identity") == true)
            {
                continue;
            }

            // ============================================
            // BLOCCO 4: AUDIT TRAIL - Prepara OldValues
            // ============================================
            string oldValues = string.Empty;

            // Se l'entità è stata modificata, cattura i valori originali
            if (entry.State == EntityState.Modified)
            {
                var originalValues = new Dictionary<string, object?>();
                foreach (var property in entry.OriginalValues.Properties)
                {
                    originalValues[property.Name] = entry.OriginalValues[property];
                }
                oldValues = JsonSerializer.Serialize(originalValues);
            }

            // ============================================
            // BLOCCO 5: AUDIT TRAIL - Prepara NewValues
            // ============================================
            string newValues = string.Empty;

            // Se l'entità non è stata eliminata, cattura i valori attuali
            if (entry.State != EntityState.Deleted)
            {
                var currentValues = new Dictionary<string, object?>();
                foreach (var property in entry.CurrentValues.Properties)
                {
                    currentValues[property.Name] = entry.CurrentValues[property];
                }
                newValues = JsonSerializer.Serialize(currentValues);
            }

            // ============================================
            // BLOCCO 6: AUDIT TRAIL - Crea AuditEntry
            // ============================================
            var auditEntry = new AuditEntry(
                Id: Guid.NewGuid(),
                User: CurrentUserId ?? "System",  // Usa CurrentUserId se disponibile, altrimenti "System"
                Action: entry.State.ToString(),   // "Added", "Modified", "Deleted"
                Entity: entityType.Name,           // Nome della classe (es. "SensorReading")
                When: DateTime.UtcNow,            // Timestamp UTC
                OldValues: oldValues,             // JSON con valori originali
                NewValues: newValues              // JSON con valori nuovi
            );

            // Aggiungi alla lista
            auditEntries.Add(auditEntry);
        }

        // ============================================
        // BLOCCO 7: AUDIT TRAIL - Aggiungi AuditEntry al ChangeTracker
        // ============================================
        // Aggiungi tutti gli AuditEntry al ChangeTracker
        // così verranno salvati insieme alle altre modifiche
        foreach (var auditEntry in auditEntries)
        {
            Entry(auditEntry).State = EntityState.Added;
        }

        // ============================================
        // BLOCCO 8: Salva tutte le modifiche
        // ============================================
        // Salva tutto: modifiche originali + AuditEntry creati
        return await base.SaveChangesAsync(cancellationToken);
    }
}