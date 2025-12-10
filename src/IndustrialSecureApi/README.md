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


### Struttura Progetto

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


