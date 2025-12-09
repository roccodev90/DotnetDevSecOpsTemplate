using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using AspNetCoreRateLimit;

using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;

using IndustrialSecureApi.Infrastructure;
using IndustrialSecureApi.Features.Auth;
using IndustrialSecureApi.Features.Sensors;
using IndustrialSecureApi.Features.Auth.Dtos;
using IndustrialSecureApi.Features.Sensors.Dtos;
using IndustrialSecureApi.Features.Sensors.Validators;
using IndustrialSecureApi.Features.Auth.Services.Interfaces;
using IndustrialSecureApi.Features.Auth.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreateSensorReadingDtoValidator>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

// Rate Limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.EnableEndpointRateLimiting = true;
    options.StackBlockedRequests = false;
    options.HttpStatusCode = 429;
    options.RealIpHeader = "X-Real-IP";
    options.ClientIdHeader = "X-ClientId";
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "*",
            Period = "1m",
            Limit = 10
        }
    };
});

builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
builder.Services.AddInMemoryRateLimiting();

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

// JWT Configuration
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// JWT Service
builder.Services.AddScoped<IJwtService, JwtService>();

// TOTP Service
builder.Services.AddScoped<ITotpService, TotpService>();

var app = builder.Build();

// Seed ruoli all'avvio
using (var scope = app.Services.CreateScope())
{
    await DataSeeder.SeedRolesAsync(scope.ServiceProvider);
}

// Rate Limiting Middleware 
app.UseIpRateLimiting();

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
app.MapPost("/readings", async (
    CreateSensorReadingDto dto,
    ApplicationDbContext context) =>
{
    var reading = new SensorReading(
        Id: Guid.NewGuid(),
        Tag: dto.Tag,
        Value: dto.Value,
        Timestamp: dto.Timestamp
    );

    context.SensorReadings.Add(reading);
    await context.SaveChangesAsync();

    return Results.Created($"/readings/{reading.Id}", reading);
})
.RequireAuthorization(policy => policy.RequireRole("Operator"));

app.MapDelete("/readings/{id}", (Guid id) => Results.Ok($"Reading {id} deleted"))
    .RequireAuthorization(policy => policy.RequireRole("Manager"));

// Endpoint per refresh token
app.MapPost("/api/auth/refresh", async (
    HttpRequest request,
    IJwtService jwtService,
    UserManager<ApplicationUser> userManager) =>
{
    var body = await request.ReadFromJsonAsync<RefreshTokenDto>();
    if (body == null || string.IsNullOrEmpty(body.RefreshToken))
        return Results.BadRequest("Refresh token richiesto");

    // Verifica refresh token nel database
    var refreshToken = await jwtService.GetRefreshTokenAsync(body.RefreshToken);
    if (refreshToken == null)
        return Results.Unauthorized();

    // Revoca il vecchio refresh token
    await jwtService.RevokeRefreshTokenAsync(body.RefreshToken);

    // Genera nuovo access token
    var user = refreshToken.User;
    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.UserName ?? ""),
        new Claim(ClaimTypes.Email, user.Email ?? "")
    };

    // Aggiungi ruoli
    var roles = await userManager.GetRolesAsync(user);
    foreach (var role in roles)
    {
        claims.Add(new Claim(ClaimTypes.Role, role));
    }

    var newAccessToken = jwtService.GenerateAccessToken(claims);

    // Genera nuovo refresh token (7 giorni)
    var newRefreshToken = jwtService.GenerateRefreshToken();
    await jwtService.SaveRefreshTokenAsync(user.Id, newRefreshToken, DateTime.UtcNow.AddDays(7));

    return Results.Ok(new
    {
        AccessToken = newAccessToken,
        RefreshToken = newRefreshToken,
        ExpiresIn = 900 // 15 minuti in secondi
    });
});

app.Run();