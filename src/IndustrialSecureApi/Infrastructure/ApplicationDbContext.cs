using Microsoft.EntityFrameworkCore;

namespace IndustrialSecureApi.Infrastructure;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Proprietà per tracciare l'utente corrente (per audit)
    public string? CurrentUserId { get; set; }

    // Aggiungeremo i DbSet dopo aver creato i modelli

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurazione indici e altro verrà aggiunta qui
        // Per ora lasciamo vuoto, lo completeremo quando avremo i modelli
    }

    // QUI INSERISCI SaveChangesAsync
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Soft Delete: marca come eliminati invece di rimuovere
        var deletedEntries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Deleted)
            .ToList();

        foreach (var entry in deletedEntries)
        {
            // Controlla se l'entità ha una proprietà "IsDeleted" o "DeletedAt"
            // Per ora lasciamo vuoto, lo completeremo quando avremo i modelli
        }

        // Audit Trail: traccia modifiche
        var modifiedEntries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added ||
                       e.State == EntityState.Modified ||
                       e.State == EntityState.Deleted)
            .ToList();

        // Per ora lasciamo vuoto, lo completeremo quando avremo AuditEntry

        return await base.SaveChangesAsync(cancellationToken);
    }
}