# Industrial Secure API

Minimal REST API per la gestione di sensori industriali (temperatura, pressione) con autenticazione a 2 fattori, autorizzazione RBAC, audit trail immutabile e pipeline DevSecOps.

## 🚀 Quick Start

### Prerequisiti
- .NET 8 SDK
- PostgreSQL 16+ (locale o Docker)
- Docker Desktop (opzionale, per container PostgreSQL)

### Setup Database

#### Opzione 1: Docker (Raccomandato per sviluppo)
```bash
# Dalla root del progetto
docker-compose up -d postgres
```

#### Opzione 2: PostgreSQL Locale
1. Crea il database `industrial_secure` in PostgreSQL
2. Aggiorna `appsettings.json` con le credenziali corrette

### Eseguire Migrations

#### Metodo 1: EF Core CLI
```bash
cd src/IndustrialSecureApi
dotnet ef database update
```

#### Metodo 2: Script SQL Manuale
1. Genera lo script: `dotnet ef migrations script --output migration.sql`
2. Esegui lo script in pgAdmin4 o psql

### Avviare l'Applicazione
```bash
cd src/IndustrialSecureApi
dotnet run
```

L'API sarà disponibile su `https://localhost:5001` o `http://localhost:5000`

## 📋 Caratteristiche

- ✅ **Database PostgreSQL** con EF Core
- ✅ **Row-Level Security** su AuditLogs (immutabile)
- ✅ **ASP.NET Core Identity** per autenticazione
- ✅ **Modelli Sensori** (Temperature, Pressure)
- ✅ **Audit Trail** completo

## 🔧 Configurazione

### Connection String
Modifica `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=industrial_secure;Username=postgres;Password=YOUR_PASSWORD"
  }
}
```

### Docker Compose
Il container PostgreSQL è configurato sulla porta **5433** per evitare conflitti con PostgreSQL locale.

## 📚 Documentazione Completa

Vedi [Documentazione Tecnica Completa](../../docs/INDUSTRIAL_SECURE_API_DOCUMENTATION.md) per dettagli approfonditi.

## 🏗 Struttura Progetto

```
IndustrialSecureApi/
├── Data/              # DbContext e Interceptors
├── Models/            # Entità (Sensor, SensorReading, AuditLog)
├── DTOs/              # Data Transfer Objects
├── Services/          # Business Logic (TOTP, JWT, Sensors)
├── Middleware/        # Audit Middleware
├── Extensions/        # Extension Methods
└── Migrations/        # EF Core Migrations
```

## 🔒 Sicurezza

- **Row-Level Security** abilitato su `AuditLogs`
- Policy `audit_immutable` blocca tutte le operazioni
- Audit trail immutabile a livello database

## 📦 Dipendenze Principali

- .NET 8.0
- Entity Framework Core 8.0
- Npgsql.EntityFrameworkCore.PostgreSQL 8.0
- ASP.NET Core Identity 8.0
- JWT Bearer Authentication 8.0
- Otp.NET 1.3.0
- Serilog.AspNetCore 8.0

## 🚧 Stato Implementazione

Vedi la [documentazione completa](../../docs/INDUSTRIAL_SECURE_API_DOCUMENTATION.md#stato-dellimplementazione) per lo stato dettagliato.

**Completato:**
- ✅ Struttura progetto
- ✅ Database e migrations
- ✅ Row-Level Security
- ✅ Modelli dati

**In sviluppo:**
- 🚧 Autenticazione 2FA
- 🚧 JWT tokens
- 🚧 RBAC
- 🚧 API endpoints

## 📝 Note

- Il database può essere creato in PostgreSQL locale (porta 5432) o Docker (porta 5433)
- Le migrations possono essere eseguite via EF Core CLI o script SQL manuale
- Row-Level Security su AuditLogs rende la tabella immutabile a livello database

