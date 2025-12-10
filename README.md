# DotnetDevSecOpsTemplate

Template .NET 8 con pipeline DevSecOps integrata (CI/CD, SAST, SCA, aggiornamenti automatici).

## Badges

[![CI](https://github.com/roccodev90/DotnetDevSecOpsTemplate/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/roccodev90/DotnetDevSecOpsTemplate/actions/workflows/ci.yml)
[![CodeQL](https://github.com/roccodev90/DotnetDevSecOpsTemplate/actions/workflows/codeql.yml/badge.svg?branch=main)](https://github.com/roccodev90/DotnetDevSecOpsTemplate/actions/workflows/codeql.yml)
[![Security scan](https://github.com/roccodev90/DotnetDevSecOpsTemplate/actions/workflows/trivy.yml/badge.svg?branch=main)](https://github.com/roccodev90/DotnetDevSecOpsTemplate/actions/workflows/trivy.yml)
[![Dependabot](https://img.shields.io/badge/dependencies-up%20to%20date-brightgreen.svg)](https://github.com/roccodev90/DotnetDevSecOpsTemplate/security/dependabot)

---

## Progetto: Industrial Secure API

REST API minimal per la gestione di sensori industriali con autenticazione a 2 fattori, autorizzazione RBAC, audit trail immutabile e pipeline DevSecOps completa.

### Caratteristiche Implementate

**Autenticazione e Autorizzazione:**
- ASP.NET Core Identity con supporto ruoli (Operator, Manager)
- Autenticazione JWT Bearer con access token e refresh token
- TOTP (Time-based One-Time Password) per 2FA
- RBAC (Role-Based Access Control)

**Sicurezza:**
- Input validation con FluentValidation
- Rate limiting (10 richieste/minuto per IP)
- Audit trail immutabile con Row-Level Security su PostgreSQL
- Dependency scanning automatico (fallisce build se vulnerabilità HIGH)
- Scansioni sicurezza: Trivy (container + filesystem), CodeQL (SAST)

**Infrastruttura:**
- Database PostgreSQL 16 con EF Core
- Docker multi-stage build (sdk:8.0 → aspnet:8.0-alpine)
- Docker Compose con orchestrazione completa (app, postgres, redis, seq)
- User non-root nel container (UID 1000)

**Logging e Monitoring:**
- Serilog con sink multipli (Console, File JSON, Seq)
- Error logging middleware automatico per 4xx/5xx
- Logging strutturato con rotazione giornaliera

**Testing:**
- Test unitari (validatori, servizi JWT/TOTP)
- Test di integrazione con WebApplicationFactory
- Database in-memory per test
- Coverage reporting

**CI/CD:**
- GitHub Actions pipeline completa
- Build automatico su push/PR
- Test con coverage
- Security scanning (Trivy, CodeQL)
- Dependency audit (blocca build se vulnerabilità HIGH)
- Docker image build e publish su GitHub Container Registry

### Quick Start

**Prerequisiti:**
- .NET 8 SDK
- Docker Desktop (per docker-compose)
- PostgreSQL 16+ (opzionale, se non usi Docker)

**Avvio con Docker Compose:**
```bash
# Avvia tutti i servizi (app, postgres, redis, seq)
docker-compose up -d

# L'API sarà disponibile su http://localhost:8080
# Seq UI su http://localhost:5342
```

**Avvio locale:**
```bash
# 1. Avvia solo PostgreSQL
docker-compose up -d postgres

# 2. Esegui migrations
cd src/IndustrialSecureApi
dotnet ef database update

# 3. Avvia l'applicazione
dotnet run
```

### Struttura Progetto

```
src/IndustrialSecureApi/
├── Features/                    # Funzionalità business
│   ├── Auth/                   # Autenticazione e autorizzazione
│   │   ├── Dtos/               # Data Transfer Objects
│   │   └── Services/            # Servizi (JWT, TOTP)
│   └── Sensors/                # Feature sensori
│       ├── Dtos/
│       └── Validators/          # Validatori FluentValidation
├── Infrastructure/
│   ├── Data/                   # ApplicationDbContext
│   ├── Models/                 # Modelli dominio e Identity
│   ├── Middleware/             # Middleware custom
│   └── Seeders/                # Inizializzazione dati
├── Dockerfile                  # Multi-stage Docker build
└── program.cs                  # Entry point

tests/IndustrialSecureApi.Tests/
├── Unit/                       # Test unitari
└── Integration/                # Test di integrazione

scripts/
└── audit.ps1                   # Script dependency scanning

.github/workflows/
├── ci.yml                      # CI pipeline (build, test, scan)
├── codeql.yml                  # CodeQL SAST
└── trivy.yml                   # Trivy security scan
```

### Endpoint API

**Autenticazione:**
- `POST /api/auth/enable-2fa` - Abilita 2FA per utente
- `POST /api/auth/verify-2fa` - Verifica codice TOTP
- `POST /api/auth/refresh` - Refresh JWT token

**Sensori:**
- `POST /readings` - Crea nuova lettura sensore (richiede ruolo Operator o Manager)
- `GET /readings` - Lista letture (da implementare)
- `GET /readings/{id}` - Dettaglio lettura (da implementare)

### Configurazione

**Connection String (appsettings.json):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=industrial_secure;Username=postgres;Password=postgres123"
  }
}
```

**JWT Configuration:**
```json
{
  "Jwt": {
    "Key": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "IndustrialSecureAPI",
    "Audience": "IndustrialSecureAPI"
  }
}
```

**Rate Limiting:**
- 10 richieste per minuto per IP
- Configurabile in `appsettings.json`

### Dependency Scanning

**Script di audit:**
```bash
# PowerShell
.\scripts\audit.ps1

# Bash
./scripts/audit.sh
```

Lo script fallisce il build se trova vulnerabilità HIGH o CRITICAL.

**Verifica manuale:**
```bash
dotnet list package --vulnerable --include-transitive
```

### Testing

**Eseguire i test:**
```bash
dotnet test
```

**Test con coverage:**
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### CI/CD Pipeline

La pipeline GitHub Actions esegue automaticamente:

1. **Build** - Compilazione progetto
2. **Test** - Esecuzione test con coverage
3. **Dependency Audit** - Scan vulnerabilità (blocca se HIGH)
4. **Security Scan** - Trivy (container + filesystem), CodeQL (SAST)
5. **Publish Image** - Build e push Docker image su GHCR (solo su main)

### Sicurezza

**Vulnerabilità risolte:**
- Npgsql 8.0.0 (CVE-2024-32655) - aggiornato a 8.0.3+
- Microsoft.Extensions.Caching.Memory 8.0.0 (DoS) - aggiornato a 8.0.2+

**Vulnerabilità Moderate rimanenti (opzionali):**
- Microsoft.IdentityModel.JsonWebTokens 7.0.3
- System.IdentityModel.Tokens.Jwt 7.0.3

### Dipendenze Principali

- .NET 8.0
- Entity Framework Core 8.0
- Npgsql.EntityFrameworkCore.PostgreSQL 8.0
- ASP.NET Core Identity 8.0
- Microsoft.AspNetCore.Authentication.JwtBearer 8.0
- FluentValidation 12.1.1
- AspNetCoreRateLimit 5.0.0
- Serilog.AspNetCore 10.0.0
- Otp.NET 1.3.0
- QRCoder 1.7.0

### Documentazione

Per la documentazione tecnica completa, vedi `docs/IndustrialSecureApi-Architecture.md` (non committata, esclusa da .gitignore).

### Stato Implementazione

**Completato:**
- Struttura progetto features-based
- Database PostgreSQL con EF Core e migrations
- Row-Level Security su AuditLogs (immutabile)
- Autenticazione JWT con refresh tokens
- TOTP per 2FA
- RBAC con ruoli Operator e Manager
- Input validation con FluentValidation
- Rate limiting
- Audit trail automatico
- Logging strutturato (Serilog)
- Error logging middleware
- Docker multi-stage build
- Docker Compose orchestrazione
- Dependency scanning script
- CI/CD pipeline GitHub Actions
- Test unitari e integrazione

**In sviluppo:**
- Endpoint completi per sensori (GET, DELETE)
- Registrazione utente
- Login completo con password + 2FA + JWT
- Token blacklist con Redis
- Health checks endpoint
- Correlation ID per logging

### Note

- Il database può essere creato in PostgreSQL locale (porta 5432) o Docker (porta 5433)
- Le migrations possono essere eseguite via EF Core CLI o script SQL manuale
- Row-Level Security su AuditLogs rende la tabella immutabile a livello database
- La documentazione in `docs/` è esclusa da Git (.gitignore)
- I secrets devono essere configurati in GitHub Actions (SONAR_TOKEN per SonarCloud)

---

## Contenuto Template

- **CI** - Build e test automatici con GitHub Actions
- **CodeQL** - Analisi statica del codice (SAST)
- **Trivy** - Scansione sicurezza (SCA + container + IaC)
- **Dependabot** - Aggiornamento automatico dipendenze
- **Docker** - Containerizzazione multi-stage
- **Docker Compose** - Orchestrazione servizi
- **Dependency Scanning** - Audit automatico vulnerabilità

## Security Controls - OWASP Top 10 Mapping

### A01:2021 - Broken Access Control
- **Implementato**: RBAC con ruoli Operator e Manager
- **Implementato**: JWT Bearer Authentication con validazione token
- **Implementato**: Endpoint protetti con `RequireAuthorization`
- **Implementato**: Refresh token con revoca

### A02:2021 - Cryptographic Failures
- **Implementato**: Password hashing con ASP.NET Core Identity (PBKDF2)
- **Implementato**: JWT con chiave segreta configurata
- **Implementato**: Secrets in Azure Key Vault (non hardcoded)
- **Implementato**: HTTPS enforcement (in produzione)

### A03:2021 - Injection
- **Implementato**: EF Core con parameterized queries (previene SQL injection)
- **Implementato**: FluentValidation per input validation
- **Implementato**: Row-Level Security su PostgreSQL per audit trail

### A04:2021 - Insecure Design
- **Implementato**: Audit trail immutabile (Row-Level Security)
- **Implementato**: Rate limiting per prevenire abusi
- **Implementato**: 2FA con TOTP

### A05:2021 - Security Misconfiguration
- **Implementato**: User non-root nel container Docker
- **Implementato**: Secrets management con Azure Key Vault
- **Implementato**: Managed Identity (no password hardcoded)
- **Implementato**: Dependency scanning automatico

### A06:2021 - Vulnerable and Outdated Components
- **Implementato**: Dependency audit script (blocca build se HIGH)
- **Implementato**: Dependabot per aggiornamenti automatici
- **Implementato**: Trivy scan per vulnerabilità container
- **Implementato**: CodeQL per SAST

### A07:2021 - Identification and Authentication Failures
- **Implementato**: ASP.NET Core Identity con password policy
- **Implementato**: JWT con expiration e refresh tokens
- **Implementato**: TOTP per 2FA
- **Implementato**: Account lockout su tentativi falliti

### A08:2021 - Software and Data Integrity Failures
- **Implementato**: CI/CD pipeline con test automatici
- **Implementato**: Code signing (opzionale)
- **Implementato**: Immutabilità audit trail (Row-Level Security)

### A09:2021 - Security Logging and Monitoring Failures
- **Implementato**: Serilog con logging strutturato
- **Implementato**: Error logging middleware (4xx/5xx)
- **Implementato**: Audit trail completo di tutte le operazioni
- **Implementato**: Logging su Seq per analisi

### A10:2021 - Server-Side Request Forgery (SSRF)
- **Implementato**: Validazione input con FluentValidation
- **Implementato**: Rate limiting per prevenire abusi
- **Nota**: Nessuna funzionalità che accetta URL esterni
