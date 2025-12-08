using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IndustrialSecureApi.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
{
    // Configurazione password
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;

    // Configurazione lockout
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // Configurazione utente
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

var app = builder.Build();

// Seed ruoli
using (var scope = app.Services.CreateScope())
{
    await DataSeeder.SeedRolesAsync(scope.ServiceProvider);
}

// Middleware per autenticazione e autorizzazione
app.UseAuthentication();
app.UseAuthorization();

// Endpoint protetti
app.MapPost("/readings", () => Results.Ok("Reading created"))
    .RequireAuthorization(policy => policy.RequireRole("Operator"));

app.MapDelete("/readings/{id}", (Guid id) => Results.Ok($"Reading {id} deleted"))
    .RequireAuthorization(policy => policy.RequireRole("Manager"));

app.MapGet("/", () => "Hello World!");

app.Run();