using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IndustrialSecureApi.Infrastructure;
using IndustrialSecureApi.Features.Auth;
using Microsoft.AspNetCore.Http;

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

// TOTP Service
builder.Services.AddScoped<ITotpService, TotpService>();

var app = builder.Build();

// Seed ruoli all'avvio
using (var scope = app.Services.CreateScope())
{
    await DataSeeder.SeedRolesAsync(scope.ServiceProvider);
}

// Middleware per autenticazione e autorizzazione
app.UseAuthentication();
app.UseAuthorization();

// Endpoint base
app.MapGet("/", () => "Hello World!");

// Endpoint per abilitare 2FA
app.MapPost("/api/auth/enable-2fa", async (
    HttpContext context,
    ITotpService totpService,
    UserManager<ApplicationUser> userManager) =>
{
    var user = await userManager.GetUserAsync(context.User);
    if (user == null)
        return Results.Unauthorized();

    // Genera secret se non esiste
    if (string.IsNullOrEmpty(user.TotpSecret))
    {
        user.TotpSecret = totpService.GenerateSecret();
        await userManager.UpdateAsync(user);
    }

    // Genera URI per QR code
    var qrCodeUri = totpService.GetQrCodeUri(user.Email ?? user.UserName ?? "", user.TotpSecret);

    return Results.Ok(new
    {
        Secret = user.TotpSecret,
        QrCodeUri = qrCodeUri,
        Message = "Scansiona il QR code con Google Authenticator"
    });
})
.RequireAuthorization();

// Endpoint per verificare codice TOTP
// Endpoint per verificare codice TOTP
app.MapPost("/api/auth/verify-2fa", async (
    HttpRequest request,
    ITotpService totpService,
    UserManager<ApplicationUser> userManager) =>
{
    // Leggi username e codice dal body
    var body = await request.ReadFromJsonAsync<Verify2FADto>();
    if (body == null || string.IsNullOrEmpty(body.Username) || string.IsNullOrEmpty(body.Code))
        return Results.BadRequest("Username e codice richiesti");

    var user = await userManager.FindByNameAsync(body.Username);
    if (user == null || !user.TotpEnabled || string.IsNullOrEmpty(user.TotpSecret))
        return Results.Unauthorized(); // ← Rimuovi il parametro

    // Valida codice TOTP
    if (!totpService.ValidateCode(user.TotpSecret, body.Code))
        return Results.Unauthorized(); // ← Rimuovi il parametro

    return Results.Ok(new { Message = "Codice TOTP valido" });
});

// Endpoint protetti
app.MapPost("/readings", () => Results.Ok("Reading created"))
    .RequireAuthorization(policy => policy.RequireRole("Operator"));

app.MapDelete("/readings/{id}", (Guid id) => Results.Ok($"Reading {id} deleted"))
    .RequireAuthorization(policy => policy.RequireRole("Manager"));

app.Run();