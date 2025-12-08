using IndustrialSecureApi.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace IndustrialSecureApi.Infrastructure;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public string? CurrentUserId { get; set; }

    // Aggiungeremo i DbSet dopo aver creato i modelli

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurazione indici e altro verrà aggiunta qui
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Soft Delete: marca come eliminati invece di rimuovere
        var deletedEntries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Deleted)
            .ToList();

        foreach (var entry in deletedEntries)
        {
            // Completeremo quando avremo i modelli
        }

        // Audit Trail: traccia modifiche
        var modifiedEntries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added ||
                       e.State == EntityState.Modified ||
                       e.State == EntityState.Deleted)
            .ToList();

        // Completeremo quando avremo AuditEntry

        return await base.SaveChangesAsync(cancellationToken);
    }
}