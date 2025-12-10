# Industrial Secure API - Mappa di Navigazione del Codice

**Versione:** 1.2.0  
**Stato:** Release Workflow Implemented  
**Ultimo aggiornamento:** Dicembre 2024

---

## 🗺️ Guida alla Navigazione

Questa documentazione è una **mappa** per orientarsi nel codice. Ti guida attraverso la struttura del progetto, i flussi di esecuzione e le responsabilità di ogni componente.

**Come usare questa mappa:**
- Inizia dalla **Struttura del Progetto** per capire dove si trova ogni componente
- Usa i **Flussi di Esecuzione** per capire come funziona un processo end-to-end
- Consulta le **Responsabilità** per capire cosa fa ogni classe/servizio
- Segui i **Percorsi di Navigazione** per trovare rapidamente il codice correlato

---

## Struttura del Progetto

### Organizzazione Features-Based

Il progetto è organizzato seguendo il pattern **Features-Based**, dove ogni feature (funzionalità) ha la sua cartella con tutto ciò che le serve.

```
src/IndustrialSecureApi/
├── Features/                    # Funzionalità business
│   ├── Auth/                   # Autenticazione e autorizzazione
│   │   ├── Dtos/               # Data Transfer Objects per Auth
│   │   └── Services/           # Servizi di autenticazione
│   │       ├── Interfaces/     # Contratti dei servizi
│   │       └── Implementations/# Implementazioni concrete
│   └── Sensors/                # Feature sensori
│       ├── Dtos/               # Data Transfer Objects per Sensors
│       └── Validators/         # Validatori FluentValidation
│
├── Infrastructure/              # Componenti infrastrutturali
│   ├── Data/                   # Accesso ai dati
│   ├── Models/                 # Modelli di dominio e Identity
│   ├── Middleware/             # Middleware custom
│   └── Seeders/                 # Inizializzazione dati
│
├── program.cs                   # Entry point e configurazione
├── ProgramMarker.cs            # Classe marker per WebApplicationFactory
└── Dockerfile                  # Multi-stage Docker build

docker-compose.yml              # Orchestrazione servizi (app, postgres, redis, seq)

infra/                          # Infrastructure as Code (Terraform)
├── main.tf                     # Provider e configurazione base
├── keyvault.tf                 # Azure Key Vault resource
├── secrets.tf                  # Secrets nel Key Vault
├── managed-identity.tf         # Managed Identity e permissions
├── variables.tf                # Variabili Terraform
├── outputs.tf                  # Output values
├── terraform.tfvars.example    # Esempio di configurazione
└── README-IaC.md               # Documentazione IaC

scripts/
└── audit.ps1                   # Script dependency scanning

tests/IndustrialSecureApi.Tests/
├── Unit/                        # Test unitari
│   ├── Validators/              # Test validatori FluentValidation
│   ├── Services/                # Test servizi (JWT, TOTP)
│   └── Policies/                # Test policy (futuro)
│
└── Integration/                 # Test di integrazione
    ├── ApiTests.cs             # Test endpoint API
    └── TestHelpers/             # Helper per test
        └── CustomWebApplicationFactory.cs  # Factory per test in-memory
```

---

## Entry Point: program.cs

**Dove:** `src/IndustrialSecureApi/program.cs`

**Cosa fa:**
- Configura Serilog per logging strutturato (Console, File, Seq)
- Configura tutti i servizi dell'applicazione
- Registra il database (PostgreSQL via EF Core)
- Configura ASP.NET Core Identity
- Configura JWT Bearer Authentication
- Configura FluentValidation per validazione input
- Configura Rate Limiting (10 req/min per IP)
- Configura Swagger/OpenAPI con documentazione XML
- Registra i servizi custom (JwtService, TotpService)
- Definisce tutti gli endpoint API con documentazione
- Esegue il seed dei ruoli all'avvio
- Configura middleware per logging errori

**Percorsi di navigazione:**
- Per vedere la configurazione Serilog → cerca `Log.Logger` all'inizio del file
- Per vedere la configurazione Identity → cerca `AddIdentity`
- Per vedere la configurazione JWT → cerca `AddJwtBearer`
- Per vedere la configurazione FluentValidation → cerca `AddValidatorsFromAssemblyContaining`
- Per vedere la configurazione Rate Limiting → cerca `IpRateLimitOptions`
- Per vedere il middleware di logging → cerca `UseMiddleware<ErrorLoggingMiddleware>`
- Per vedere gli endpoint → scorri verso il basso dopo `var app = builder.Build()`
- Per vedere i servizi registrati → cerca `AddScoped`

**Flusso di avvio:**
1. Configura Serilog (Console, File, Seq)
2. Crea il builder dell'applicazione
3. Configura i servizi (database, Identity, JWT, FluentValidation, Rate Limiting)
4. Builda l'applicazione
5. Esegue il seed dei ruoli (Operator, Manager)
6. Configura middleware (error logging, rate limiting, autenticazione, autorizzazione)
7. Definisce gli endpoint
8. Avvia il server con gestione errori e chiusura Serilog

---

## Feature: Autenticazione (Auth)

### Struttura

**Dove:** `src/IndustrialSecureApi/Features/Auth/`

**Contiene:**
- **Dtos/**: Oggetti per trasferire dati (RefreshTokenDto, Verify2FADto)
- **Services/Interfaces/**: Contratti dei servizi (IJwtService, ITotpService)
- **Services/Implementations/**: Implementazioni concrete (JwtService, TotpService)

### Servizi di Autenticazione

#### TotpService

**Dove:** `Features/Auth/Services/Implementations/TotpService.cs`

**Responsabilità:**
- Genera chiavi segrete TOTP (Time-based One-Time Password)
- Valida codici TOTP inseriti dall'utente
- Genera URI per QR code compatibili con Google Authenticator

**Come funziona:**
- Usa la libreria `Otp.NET` per la logica TOTP
- Genera chiavi Base32 di 20 byte (160 bit)
- Valida codici con tolleranza di ±1 periodo (30 secondi)
- Genera URI nel formato standard `otpauth://totp/...`

**Quando viene usato:**
- Quando un utente abilita 2FA (endpoint `/api/auth/enable-2fa`)
- Quando un utente verifica un codice TOTP (endpoint `/api/auth/verify-2fa`)

**Percorsi di navigazione:**
- Interfaccia → `Features/Auth/Services/Interfaces/ITotpService.cs`
- Utilizzo negli endpoint → `program.cs` (cerca `ITotpService`)

#### JwtService

**Dove:** `Features/Auth/Services/Implementations/JwtService.cs`

**Responsabilità:**
- Genera access token JWT (validità 15 minuti)
- Genera refresh token (validità 7 giorni)
- Valida token JWT
- Gestisce refresh token nel database (salvataggio, revoca, recupero)

**Come funziona:**
- Access token: contiene claims dell'utente (ID, username, email, ruoli), firmato con HMAC-SHA256
- Refresh token: stringa random Base64 salvata nel database con scadenza
- Validazione: verifica firma, issuer, audience, scadenza
- Revoca: marca il refresh token come revocato nel database

**Quando viene usato:**
- Quando un utente fa login (genera access + refresh token)
- Quando un utente rinnova il token (endpoint `/api/auth/refresh`)
- Ad ogni richiesta autenticata (validazione automatica via middleware)

**Percorsi di navigazione:**
- Interfaccia → `Features/Auth/Services/Interfaces/IJwtService.cs`
- Configurazione JWT → `program.cs` (cerca `AddJwtBearer`)
- Utilizzo negli endpoint → `program.cs` (cerca `IJwtService`)

### DTOs

**Dove:** `Features/Auth/Dtos/`

**RefreshTokenDto:**
- Contiene il refresh token da rinnovare
- Usato nell'endpoint `/api/auth/refresh`

**Verify2FADto:**
- Contiene username e codice TOTP
- Usato nell'endpoint `/api/auth/verify-2fa`

---

## Feature: Sensori (Sensors)

### Struttura

**Dove:** `src/IndustrialSecureApi/Features/Sensors/`

**Contiene:**
- **Dtos/**: Data Transfer Objects per creare letture sensori (CreateSensorReadingDto)
- **Validators/**: Validatori FluentValidation per validazione input

### CreateSensorReadingDto

**Dove:** `Features/Sensors/Dtos/CreateSensorReadingDto.cs`

**Responsabilità:**
- Rappresenta i dati necessari per creare una nuova lettura sensore
- Usato nell'endpoint `POST /readings`

**Contiene:**
- Tag: Identificativo del sensore (obbligatorio, max 100 caratteri)
- Value: Valore della lettura (obbligatorio, range -50 a 200)
- Timestamp: Data e ora della lettura (obbligatorio)

**Percorsi di navigazione:**
- Utilizzo → endpoint `POST /readings` in `program.cs`
- Validazione → `Features/Sensors/Validators/CreateSensorReadingDtoValidator.cs`

### CreateSensorReadingDtoValidator

**Dove:** `Features/Sensors/Validators/CreateSensorReadingDtoValidator.cs`

**Responsabilità:**
- Valida i dati di input per la creazione di letture sensori
- Usa FluentValidation per definire le regole di validazione

**Regole implementate:**
- Tag: Non vuoto, massimo 100 caratteri
- Value: Compreso tra -50 e 200 (inclusi)
- Timestamp: Non vuoto

**Come funziona:**
- FluentValidation viene eseguito automaticamente quando il DTO viene ricevuto dall'endpoint
- Se la validazione fallisce, viene restituito un errore 400 Bad Request con i dettagli degli errori
- Se la validazione passa, l'endpoint procede con la logica

**Percorsi di navigazione:**
- Configurazione → `program.cs` (cerca `AddValidatorsFromAssemblyContaining`)
- Utilizzo → automatico quando viene ricevuto `CreateSensorReadingDto` in un endpoint

---

## Infrastructure: Accesso ai Dati

### ApplicationDbContext

**Dove:** `Infrastructure/Data/ApplicationDbContext.cs`

**Responsabilità:**
- Gestisce la connessione al database PostgreSQL
- Configura le entità (chiavi, indici, relazioni)
- Implementa audit trail automatico
- Gestisce soft delete (preparato per futuro)

**Cosa contiene:**
- **DbSet<SensorReading>**: Letture dei sensori
- **DbSet<AuditEntry>**: Log di audit
- **DbSet<UserRefreshToken>**: Refresh token degli utenti
- Eredita da `IdentityDbContext` per le tabelle Identity (Users, Roles, etc.)

**Audit Trail Automatico:**
Il metodo `SaveChangesAsync` è sovrascritto per tracciare automaticamente tutte le modifiche:
1. Intercetta tutte le entità modificate (Added, Modified, Deleted)
2. Esclude entità Identity (per evitare log eccessivi)
3. Esclude AuditEntry stesso (evita loop infiniti)
4. Serializza valori originali e nuovi in JSON
5. Crea un AuditEntry per ogni modifica
6. Salva tutto in una transazione

**Proprietà CurrentUserId:**
- Permette di tracciare quale utente ha fatto la modifica
- Se non impostato, usa "System" come fallback
- Può essere impostato prima di chiamare SaveChangesAsync

**Percorsi di navigazione:**
- Configurazione entità → metodo `OnModelCreating`
- Audit trail → metodo `SaveChangesAsync`
- Utilizzo → iniettato via dependency injection nei servizi

---

## Infrastructure: Modelli

**Dove:** `Infrastructure/Models/`

### ApplicationUser

**Responsabilità:**
- Estende `IdentityUser<Guid>` di ASP.NET Core Identity
- Aggiunge supporto per TOTP (chiave segreta e flag abilitazione)

**Campi aggiuntivi:**
- `TotpSecret`: Chiave segreta TOTP (nullable, generata quando l'utente abilita 2FA)
- `TotpEnabled`: Flag che indica se l'utente ha abilitato 2FA

**Percorsi di navigazione:**
- Utilizzo → `program.cs` (configurazione Identity), endpoint auth

### AuditEntry

**Responsabilità:**
- Rappresenta una voce di audit trail
- Record immutabile (C# 12)

**Contiene:**
- ID univoco
- Utente che ha fatto la modifica
- Azione (Added, Modified, Deleted)
- Nome dell'entità modificata
- Timestamp UTC
- Valori originali (JSON)
- Valori nuovi (JSON)

**Percorsi di navigazione:**
- Creazione → `ApplicationDbContext.SaveChangesAsync`
- Query → tramite `ApplicationDbContext.AuditEntries`

### UserRefreshToken

**Responsabilità:**
- Rappresenta un refresh token salvato nel database
- Permette revoca e controllo dei token attivi

**Contiene:**
- ID univoco
- UserId (relazione con ApplicationUser)
- Token (stringa Base64, univoca, indicizzata)
- Data di scadenza
- Data di creazione
- Flag di revoca

**Percorsi di navigazione:**
- Creazione → `JwtService.SaveRefreshTokenAsync`
- Revoca → `JwtService.RevokeRefreshTokenAsync`
- Query → `JwtService.GetRefreshTokenAsync`

### SensorReading

**Dove:** `Infrastructure/Models/SensorReading.cs` (namespace: `Features.Sensors`)

**Responsabilità:**
- Rappresenta una lettura di un sensore industriale
- Record immutabile (C# 12)

**Contiene:**
- ID univoco
- Tag (identificativo del sensore, max 100 caratteri, indicizzato)
- Valore (double)
- Timestamp

**Percorsi di navigazione:**
- Configurazione → `ApplicationDbContext.OnModelCreating`
- Utilizzo → endpoint `/readings` (futuro)

---

## Infrastructure: Seeders

### DataSeeder

**Dove:** `Infrastructure/Seeders/DataSeeder.cs`

**Responsabilità:**
- Inizializza i dati base del sistema all'avvio
- Crea i ruoli "Operator" e "Manager" se non esistono

**Quando viene eseguito:**
- All'avvio dell'applicazione, in `program.cs`
- Viene chiamato dopo il build dell'app ma prima dell'avvio del server

**Percorsi di navigazione:**
- Chiamata → `program.cs` (dopo `var app = builder.Build()`)

---

## Infrastructure: Middleware

**Dove:** `Infrastructure/Middleware/`

### ErrorLoggingMiddleware

**Dove:** `Infrastructure/Middleware/ErrorLoggingMiddleware.cs`

**Responsabilità:**
- Intercetta tutte le richieste HTTP
- Calcola l'hash SHA256 del body della richiesta (per privacy)
- Logga automaticamente tutti gli errori 4xx e 5xx
- Include informazioni contestuali: user, IP, status code, metodo, path, body hash

**Come funziona:**
- Viene eseguito per ogni richiesta HTTP
- Abilita il buffering del body per permettere la lettura multipla
- Calcola l'hash SHA256 del body (non logga il contenuto per privacy)
- Dopo che la richiesta è stata processata, verifica lo status code
- Se lo status code è 4xx o 5xx, logga un warning con tutte le informazioni
- Se lo status code è 2xx o 3xx, non logga nulla

**Informazioni loggate:**
- Status Code (es. 400, 401, 404, 500)
- Metodo HTTP (GET, POST, DELETE, etc.)
- Path della richiesta
- User ID (o "Anonymous" se non autenticato)
- IP Address del client
- Body Hash (SHA256 del body, Base64)

**Percorsi di navigazione:**
- Implementazione → `Infrastructure/Middleware/ErrorLoggingMiddleware.cs`
- Registrazione → `program.cs` (cerca `UseMiddleware<ErrorLoggingMiddleware>`)
- Utilizzo → automatico per tutte le richieste

---

## Infrastructure: Logging

### Serilog

**Dove:** Configurato in `program.cs`

**Responsabilità:**
- Fornisce logging strutturato per tutta l'applicazione
- Sostituisce il logger di default di ASP.NET Core
- Supporta multiple destinazioni (sinks)

**Sinks configurati:**
- **Console**: Output in console durante lo sviluppo
- **File**: Log salvati in formato JSON in `logs/app-YYYYMMDD.json`
  - Rotazione giornaliera
  - Retention di 7 giorni
  - Formato JSON strutturato
- **Seq**: Log inviati a Seq server (opzionale, localhost:5341)
  - Utile per analisi e ricerca avanzata
  - Può essere rimosso se non usato

**Come funziona:**
- Serilog viene configurato prima della creazione del builder
- Sostituisce il logger di default con `builder.Host.UseSerilog()`
- Tutti i log dell'applicazione passano attraverso Serilog
- I log vengono scritti simultaneamente su tutti i sinks configurati
- Alla chiusura dell'applicazione, Serilog viene chiuso correttamente

**Livelli di log:**
- Information: Operazioni normali (avvio applicazione, etc.)
- Warning: Errori 4xx/5xx loggati dal middleware
- Error: Errori dell'applicazione
- Fatal: Errori fatali che causano la chiusura dell'applicazione

**Percorsi di navigazione:**
- Configurazione → `program.cs` (cerca `Log.Logger` all'inizio)
- Utilizzo → automatico in tutta l'applicazione
- File di log → cartella `logs/` nella root del progetto

---

## Infrastructure: Middleware e Validazione

### FluentValidation

**Dove:** Configurato in `program.cs`

**Responsabilità:**
- Valida automaticamente i DTO ricevuti dagli endpoint
- Restituisce errori 400 Bad Request se la validazione fallisce
- Supporta validazione lato client (per future integrazioni frontend)

**Come funziona:**
- I validatori vengono registrati automaticamente dall'assembly
- Quando un endpoint riceve un DTO, FluentValidation lo valida automaticamente
- Se ci sono errori, vengono restituiti prima che l'endpoint venga eseguito
- Gli errori includono messaggi personalizzati per ogni regola violata

**Validatori disponibili:**
- `CreateSensorReadingDtoValidator`: Valida le letture sensori

**Percorsi di navigazione:**
- Configurazione → `program.cs` (cerca `AddValidatorsFromAssemblyContaining`)
- Validatori → `Features/*/Validators/`

### Rate Limiting

**Dove:** Configurato in `program.cs` e `appsettings.json`

**Responsabilità:**
- Limita il numero di richieste per IP address
- Previene abusi e attacchi DDoS
- Protegge l'API da sovraccarico

**Configurazione attuale:**
- Limite: 10 richieste per minuto per IP
- Applicato a tutti gli endpoint (`*`)
- Risposta quando il limite è superato: 429 Too Many Requests

**Come funziona:**
- Il middleware `UseIpRateLimiting` viene eseguito prima dell'autenticazione
- Traccia le richieste per IP address usando MemoryCache
- Conta le richieste in finestre temporali di 1 minuto
- Se un IP supera il limite, tutte le richieste successive in quel minuto vengono rifiutate
- Il contatore si resetta ogni minuto

**Percorsi di navigazione:**
- Configurazione → `program.cs` (cerca `IpRateLimitOptions`)
- Middleware → `program.cs` (cerca `UseIpRateLimiting`)
- Configurazione opzionale → `appsettings.json` (sezione `IpRateLimiting`)

---

## Flussi di Esecuzione

### Flusso: Abilitazione 2FA

**Endpoint:** `POST /api/auth/enable-2fa`

**Requisiti:** Utente autenticato (JWT Bearer token)

**Percorso nel codice:**
1. `program.cs` → endpoint `/api/auth/enable-2fa`
2. Estrae l'utente dal contesto HTTP (via `UserManager`)
3. `TotpService.GenerateSecret()` → genera chiave segreta
4. Salva la chiave nell'utente (via `UserManager.UpdateAsync`)
5. `TotpService.GetQrCodeUri()` → genera URI per QR code
6. Restituisce secret e QR code URI al client

**Cosa succede:**
- L'utente riceve una chiave segreta e un URI QR code
- Scansiona il QR code con Google Authenticator (o app simile)
- L'app genera codici TOTP ogni 30 secondi
- L'utente può ora verificare i codici con l'endpoint `/api/auth/verify-2fa`

### Flusso: Verifica Codice TOTP

**Endpoint:** `POST /api/auth/verify-2fa`

**Requisiti:** Nessuno (pubblico)

**Percorso nel codice:**
1. `program.cs` → endpoint `/api/auth/verify-2fa`
2. Legge username e codice dal body (Verify2FADto)
3. Trova l'utente (via `UserManager.FindByNameAsync`)
4. Verifica che l'utente abbia 2FA abilitato e secret configurato
5. `TotpService.ValidateCode()` → valida il codice TOTP
6. Restituisce successo o errore

**Cosa succede:**
- L'utente inserisce il codice TOTP generato dall'app
- Il servizio valida il codice contro la chiave segreta salvata
- Se valido, l'utente può procedere (in futuro: generare JWT token)

### Flusso: Refresh Token

**Endpoint:** `POST /api/auth/refresh`

**Requisiti:** Refresh token valido

**Percorso nel codice:**
1. `program.cs` → endpoint `/api/auth/refresh`
2. Legge refresh token dal body (RefreshTokenDto)
3. `JwtService.GetRefreshTokenAsync()` → verifica token nel database
4. Verifica che il token non sia revocato e non sia scaduto
5. `JwtService.RevokeRefreshTokenAsync()` → revoca il vecchio token
6. Estrae l'utente dal refresh token
7. Crea claims (ID, username, email, ruoli)
8. `JwtService.GenerateAccessToken()` → genera nuovo access token (15 min)
9. `JwtService.GenerateRefreshToken()` → genera nuovo refresh token
10. `JwtService.SaveRefreshTokenAsync()` → salva nuovo refresh token (7 giorni)
11. Restituisce entrambi i token al client

**Cosa succede:**
- Il client invia un refresh token scaduto o prossimo alla scadenza
- Il server verifica che il token sia valido nel database
- Revoca il vecchio token (non può essere riutilizzato)
- Genera nuovi token (access + refresh)
- Il client può continuare a usare l'API con il nuovo access token

### Flusso: Audit Trail Automatico

**Trigger:** Qualsiasi chiamata a `SaveChangesAsync` su `ApplicationDbContext`

**Percorso nel codice:**
1. Qualsiasi servizio chiama `ApplicationDbContext.SaveChangesAsync()`
2. `ApplicationDbContext.SaveChangesAsync()` (override) viene eseguito
3. Identifica tutte le entità modificate (Added, Modified, Deleted)
4. Per ogni entità:
   - Salta se è AuditEntry o entità Identity
   - Serializza valori originali (se Modified)
   - Serializza valori nuovi (se non Deleted)
   - Crea AuditEntry con tutte le informazioni
5. Aggiunge tutti gli AuditEntry al ChangeTracker
6. Chiama `base.SaveChangesAsync()` per salvare tutto in una transazione

**Cosa succede:**
- Ogni modifica a SensorReading, UserRefreshToken, etc. viene automaticamente tracciata
- Viene creato un AuditEntry con:
  - Chi ha fatto la modifica (CurrentUserId o "System")
  - Cosa è stato modificato (nome entità)
  - Come è stato modificato (Added, Modified, Deleted)
  - Quando (timestamp UTC)
  - Valori prima e dopo (JSON)
- Gli AuditEntry sono immutabili a livello database (Row-Level Security)

**Percorsi di navigazione:**
- Implementazione → `ApplicationDbContext.SaveChangesAsync`
- Query audit → tramite `ApplicationDbContext.AuditEntries`

### Flusso: Autenticazione JWT

**Trigger:** Qualsiasi richiesta a endpoint protetto

**Percorso nel codice:**
1. Client invia richiesta con header `Authorization: Bearer <token>`
2. Middleware JWT (configurato in `program.cs`) intercetta la richiesta
3. Estrae il token dall'header
4. Valida il token (firma, issuer, audience, scadenza)
5. Se valido, estrae i claims e crea un `ClaimsPrincipal`
6. Il `ClaimsPrincipal` viene associato al `HttpContext.User`
7. L'endpoint può accedere all'utente tramite `HttpContext.User` o `UserManager.GetUserAsync()`

**Cosa succede:**
- Il client deve includere un JWT token valido in ogni richiesta protetta
- Il middleware valida automaticamente il token
- Se valido, l'utente è autenticato e disponibile nel contesto
- Se non valido o mancante, la richiesta viene rifiutata con 401 Unauthorized

**Percorsi di navigazione:**
- Configurazione → `program.cs` (cerca `AddJwtBearer`)
- Utilizzo → endpoint con `.RequireAuthorization()`

### Flusso: Autorizzazione RBAC

**Trigger:** Endpoint con `.RequireAuthorization(policy => policy.RequireRole(...))`

**Percorso nel codice:**
1. Endpoint definito con autorizzazione per ruolo (es. "Operator")
2. Dopo l'autenticazione JWT, il middleware di autorizzazione verifica i claims
3. Cerca il claim `ClaimTypes.Role` nel token
4. Se il ruolo corrisponde, la richiesta procede
5. Se non corrisponde, la richiesta viene rifiutata con 403 Forbidden

**Ruoli disponibili:**
- **Operator**: Può creare letture sensori (POST /readings)
- **Manager**: Può eliminare letture sensori (DELETE /readings/{id})

**Percorsi di navigazione:**
- Definizione ruoli → `DataSeeder.SeedRolesAsync`
- Utilizzo → `program.cs` (endpoint con `.RequireAuthorization`)

### Flusso: Validazione Input con FluentValidation

**Trigger:** Qualsiasi endpoint che riceve un DTO con validatore associato

**Percorso nel codice:**
1. Client invia richiesta con DTO nel body (es. `CreateSensorReadingDto`)
2. Middleware FluentValidation intercetta la richiesta
3. Trova il validatore associato al DTO (es. `CreateSensorReadingDtoValidator`)
4. Esegue tutte le regole di validazione
5. Se ci sono errori:
   - Restituisce 400 Bad Request con lista errori
   - L'endpoint non viene eseguito
6. Se la validazione passa:
   - La richiesta procede all'endpoint
   - L'endpoint riceve il DTO già validato

**Cosa succede:**
- Ogni DTO può avere un validatore associato
- Le regole di validazione sono definite nel validatore
- Gli errori vengono restituiti in formato strutturato
- La validazione avviene automaticamente, senza codice aggiuntivo nell'endpoint

**Esempio:**
- Client invia `POST /readings` con `Value: 300`
- FluentValidation valida: `Value <= 200` fallisce
- API restituisce 400 con messaggio "Value deve essere <= 200"
- L'endpoint non viene eseguito

**Percorsi di navigazione:**
- Configurazione → `program.cs` (cerca `AddFluentValidationAutoValidation`)
- Validatori → `Features/*/Validators/`
- Utilizzo → automatico quando un endpoint riceve un DTO

### Flusso: Rate Limiting

**Trigger:** Qualsiasi richiesta HTTP all'API

**Percorso nel codice:**
1. Client invia richiesta HTTP
2. Middleware Rate Limiting (`UseIpRateLimiting`) intercetta la richiesta
3. Estrae l'IP address del client
4. Controlla quante richieste ha fatto quell'IP nell'ultimo minuto
5. Se il limite è stato superato:
   - Restituisce 429 Too Many Requests
   - La richiesta non procede oltre
6. Se il limite non è stato superato:
   - Incrementa il contatore per quell'IP
   - La richiesta procede normalmente

**Cosa succede:**
- Ogni IP può fare massimo 10 richieste al minuto
- Il contatore si resetta ogni minuto
- Le richieste vengono tracciate in memoria (MemoryCache)
- Se un IP supera il limite, deve aspettare che il minuto scada

**Esempio:**
- IP 192.168.1.100 fa 10 richieste in 30 secondi → tutte passano
- IP 192.168.1.100 fa l'11esima richiesta → 429 Too Many Requests
- Dopo 1 minuto dalla prima richiesta, il contatore si resetta
- IP 192.168.1.100 può fare nuove richieste

**Percorsi di navigazione:**
- Configurazione → `program.cs` (cerca `IpRateLimitOptions`)
- Middleware → `program.cs` (cerca `UseIpRateLimiting`)

### Flusso: Creazione Lettura Sensore

**Endpoint:** `POST /readings`

**Requisiti:** Utente autenticato con ruolo "Operator"

**Percorso nel codice:**
1. `program.cs` → endpoint `POST /readings`
2. Middleware Rate Limiting verifica il limite per IP
3. Middleware JWT autentica l'utente
4. Middleware Authorization verifica il ruolo "Operator"
5. FluentValidation valida il DTO `CreateSensorReadingDto`
6. Se la validazione passa, l'endpoint viene eseguito:
   - Crea nuovo `SensorReading` dal DTO
   - Aggiunge al `ApplicationDbContext`
   - Chiama `SaveChangesAsync` (che crea automaticamente AuditEntry)
   - Restituisce 201 Created con la lettura creata

**Cosa succede:**
- Il client invia Tag, Value, Timestamp
- Il sistema valida che Value sia tra -50 e 200
- Se valido, crea la lettura nel database
- Viene creato automaticamente un AuditEntry per tracciare la creazione
- Il client riceve la lettura creata con ID assegnato

**Percorsi di navigazione:**
- Endpoint → `program.cs` (cerca `POST /readings`)
- Validazione → `Features/Sensors/Validators/CreateSensorReadingDtoValidator.cs`
- Creazione → `ApplicationDbContext.SaveChangesAsync` (audit automatico)

### Flusso: Logging Errori HTTP

**Trigger:** Qualsiasi richiesta HTTP che restituisce status code 4xx o 5xx

**Percorso nel codice:**
1. Client invia richiesta HTTP
2. `ErrorLoggingMiddleware` intercetta la richiesta
3. Abilita buffering del body per permettere lettura multipla
4. Calcola hash SHA256 del body (se presente)
5. La richiesta procede normalmente attraverso la pipeline
6. Dopo che la risposta è stata generata, il middleware verifica lo status code
7. Se status code >= 400 e < 600:
   - Estrae informazioni: user, IP, method, path, body hash
   - Logga un warning con tutte le informazioni
8. Se status code < 400, non logga nulla

**Cosa succede:**
- Ogni errore HTTP viene automaticamente tracciato
- Le informazioni vengono loggate su Console, File (JSON) e Seq (se configurato)
- Il body viene hashato invece di essere loggato in chiaro (privacy)
- Gli errori sono facilmente ricercabili nei file di log JSON
- I log includono contesto sufficiente per il debugging

**Esempio di log:**
- Status: 400 Bad Request
- Method: POST
- Path: /readings
- User: operator1
- IP: 192.168.1.100
- BodyHash: abc123... (SHA256 Base64)

**Percorsi di navigazione:**
- Implementazione → `Infrastructure/Middleware/ErrorLoggingMiddleware.cs`
- Registrazione → `program.cs` (cerca `UseMiddleware<ErrorLoggingMiddleware>`)
- File di log → `logs/app-YYYYMMDD.json`

---

## Come Trovare il Codice

### Voglio vedere come funziona l'autenticazione JWT

1. Configurazione → `program.cs` (cerca `AddJwtBearer`)
2. Generazione token → `Features/Auth/Services/Implementations/JwtService.cs` (metodo `GenerateAccessToken`)
3. Validazione automatica → gestita dal middleware JWT (configurato in `program.cs`)

### Voglio vedere come funziona il 2FA

1. Generazione secret → `Features/Auth/Services/Implementations/TotpService.cs` (metodo `GenerateSecret`)
2. Validazione codice → `Features/Auth/Services/Implementations/TotpService.cs` (metodo `ValidateCode`)
3. Endpoint abilitazione → `program.cs` (cerca `/api/auth/enable-2fa`)
4. Endpoint verifica → `program.cs` (cerca `/api/auth/verify-2fa`)

### Voglio vedere come funziona l'audit trail

1. Implementazione → `Infrastructure/Data/ApplicationDbContext.cs` (metodo `SaveChangesAsync`)
2. Modello → `Infrastructure/Models/AuditEntry.cs`
3. Configurazione database → `ApplicationDbContext.OnModelCreating` (configurazione AuditEntry)

### Voglio vedere come sono configurati gli endpoint

1. Tutti gli endpoint → `program.cs` (dopo `var app = builder.Build()`)
2. Endpoint auth → cerca `/api/auth/`
3. Endpoint protetti → cerca `/readings`

### Voglio vedere come sono configurate le entità del database

1. Configurazione → `Infrastructure/Data/ApplicationDbContext.cs` (metodo `OnModelCreating`)
2. Modelli → `Infrastructure/Models/`

### Voglio vedere come vengono inizializzati i ruoli

1. Seeder → `Infrastructure/Seeders/DataSeeder.cs`
2. Chiamata → `program.cs` (dopo `var app = builder.Build()`)

### Voglio vedere come funziona la validazione input

1. Configurazione FluentValidation → `program.cs` (cerca `AddValidatorsFromAssemblyContaining`)
2. Validatore per SensorReading → `Features/Sensors/Validators/CreateSensorReadingDtoValidator.cs`
3. DTO validato → `Features/Sensors/Dtos/CreateSensorReadingDto.cs`
4. Utilizzo → endpoint `POST /readings` in `program.cs`

### Voglio vedere come funziona il rate limiting

1. Configurazione → `program.cs` (cerca `IpRateLimitOptions`)
2. Middleware → `program.cs` (cerca `UseIpRateLimiting`)
3. Configurazione opzionale → `appsettings.json` (sezione `IpRateLimiting`)

### Voglio vedere come funziona il logging

1. Configurazione Serilog → `program.cs` (cerca `Log.Logger` all'inizio)
2. Middleware error logging → `Infrastructure/Middleware/ErrorLoggingMiddleware.cs`
3. Registrazione middleware → `program.cs` (cerca `UseMiddleware<ErrorLoggingMiddleware>`)
4. File di log → cartella `logs/` nella root del progetto

### Voglio vedere come funzionano i test

1. Test unitari → `tests/IndustrialSecureApi.Tests/Unit/`
2. Test integrazione → `tests/IndustrialSecureApi.Tests/Integration/`
3. Factory per test → `tests/IndustrialSecureApi.Tests/Integration/TestHelpers/CustomWebApplicationFactory.cs`
4. Marker class → `src/IndustrialSecureApi/ProgramMarker.cs`

### Voglio vedere come funziona Docker

1. Dockerfile → `src/IndustrialSecureApi/Dockerfile`
2. docker-compose.yml → `docker-compose.yml` (root)
3. Build immagine: `docker build -t industrial-secure-api -f src/IndustrialSecureApi/Dockerfile .`
4. Avvia servizi: `docker-compose up -d`

### Voglio eseguire lo scan delle vulnerabilità

1. Script audit → `scripts/audit.ps1`
2. Esegui: `.\scripts\audit.ps1` (PowerShell) o `./scripts/audit.sh` (Bash)
3. Verifica manuale: `dotnet list package --vulnerable --include-transitive`

### Voglio vedere la configurazione Infrastructure as Code

1. Terraform files → `infra/` (root del repository)
2. Key Vault → `infra/keyvault.tf`
3. Secrets → `infra/secrets.tf`
4. Managed Identity → `infra/managed-identity.tf`
5. Documentazione → `infra/README-IaC.md`

### Voglio vedere la documentazione API

1. Swagger UI → `http://localhost:5000` (in Development)
2. Configurazione Swagger → `program.cs` (cerca `AddSwaggerGen`)
3. Documentazione endpoint → `program.cs` (cerca `.WithSummary`, `.WithDescription`)
4. CHANGELOG → `CHANGELOG.md` (root)
5. Security Controls → Sezione "Security Controls" in questa documentazione

### Voglio creare un release

1. Release workflow → `.github/workflows/release.yml`
2. Aggiorna CHANGELOG → `CHANGELOG.md`
3. Crea tag → `git tag -a v1.0.0 -m "Release v1.0.0"`
4. Push tag → `git push origin v1.0.0`
5. Monitora → GitHub Actions → Release workflow
6. Verifica → GitHub Releases per vedere SBOM e Docker image

---

## Architettura a Livelli

### Layer di Presentazione
- **program.cs**: Endpoint API, configurazione middleware
- **Features/*/Validators/**: Validazione input (FluentValidation)
- **Rate Limiting Middleware**: Protezione da abusi
- **Error Logging Middleware**: Logging automatico errori HTTP

### Layer di Business Logic
- **Features/Auth/Services/**: Logica di autenticazione (JWT, TOTP)
- **Features/Sensors/**: Logica sensori e validazione input

### Layer di Accesso ai Dati
- **Infrastructure/Data/ApplicationDbContext.cs**: EF Core, audit trail
- **Infrastructure/Models/**: Modelli di dominio

### Layer di Infrastruttura
- **Infrastructure/Seeders/**: Inizializzazione dati
- **Infrastructure/Middleware/**: Middleware custom (error logging)
- **Serilog**: Logging strutturato (Console, File, Seq)
- Configurazione in `program.cs`: Database, Identity, JWT, FluentValidation, Rate Limiting, Serilog

---

## Responsabilità per Componente

### program.cs
- Configurazione servizi
- Definizione endpoint
- Setup middleware
- Seed dati iniziali

### ApplicationDbContext
- Connessione database
- Configurazione entità
- Audit trail automatico

### JwtService
- Generazione e validazione token JWT
- Gestione refresh token nel database

### TotpService
- Generazione chiavi segrete TOTP
- Validazione codici TOTP
- Generazione URI QR code

### DataSeeder
- Inizializzazione ruoli

### ApplicationUser
- Estende Identity con supporto TOTP

### AuditEntry
- Rappresenta una voce di audit trail

### UserRefreshToken
- Rappresenta un refresh token nel database

### SensorReading
- Rappresenta una lettura di sensore

### CreateSensorReadingDto
- DTO per creare nuove letture sensori
- Validato da FluentValidation

### CreateSensorReadingDtoValidator
- Validatore FluentValidation per CreateSensorReadingDto
- Regole: Tag (obbligatorio, max 100), Value (-50 a 200), Timestamp (obbligatorio)

### Rate Limiting Middleware
- Limita richieste per IP (10/min)
- Protegge l'API da abusi

### ErrorLoggingMiddleware
- Logga automaticamente tutti gli errori 4xx/5xx
- Include user, IP, body hash, status code, method, path
- Calcola hash SHA256 del body per privacy

### Serilog
- Logging strutturato per tutta l'applicazione
- Sinks: Console, File (JSON), Seq (opzionale)
- Rotazione giornaliera file, retention 7 giorni

---

## Testing Infrastructure

### Struttura dei Test

Il progetto include una suite completa di test organizzata in due categorie principali:

#### Test Unitari (`tests/IndustrialSecureApi.Tests/Unit/`)

Test isolati che verificano singoli componenti senza dipendenze esterne:

- **Validators**: Test dei validatori FluentValidation
  - `CreateSensorReadingDtoValidatorTests`: Verifica regole di validazione per sensor readings
  - Testa limiti di valore (-50 a 200), validazione tag, timestamp

- **Services**: Test dei servizi di business logic
  - `JwtServiceTests`: Verifica generazione e validazione token JWT
  - `TotpServiceTests`: Verifica generazione secret, validazione codici, QR code

- **Policies**: Test delle policy di autorizzazione (futuro)

#### Test di Integrazione (`tests/IndustrialSecureApi.Tests/Integration/`)

Test end-to-end che verificano il comportamento completo dell'API:

- **ApiTests**: Test degli endpoint API
  - Testa flussi completi: richiesta HTTP → validazione → business logic → risposta
  - Usa `CustomWebApplicationFactory` per creare un'applicazione in-memory

- **TestHelpers**: Utility per facilitare i test
  - `CustomWebApplicationFactory`: Factory per creare istanze dell'applicazione per i test
  - Configura database in-memory invece di PostgreSQL
  - Mantiene tutti i servizi configurati (Identity, JWT, Authorization, etc.)

### Flussi di Esecuzione dei Test

#### Esecuzione Test Unitari

1. **Setup**: Crea istanza del componente da testare (validator, service)
2. **Arrange**: Prepara dati di input e configurazione
3. **Act**: Esegue l'operazione da testare
4. **Assert**: Verifica che il risultato sia quello atteso

**Esempio - Test Validatore:**
- Input: DTO con valore fuori range (-51)
- Operazione: Chiama `validator.Validate(dto)`
- Verifica: Risultato non valido, errore sulla proprietà `Value`

#### Esecuzione Test di Integrazione

1. **Setup**: `CustomWebApplicationFactory` crea un'istanza dell'applicazione
   - Sostituisce PostgreSQL con database in-memory
   - Mantiene tutti i servizi configurati (Identity, JWT, Authorization, etc.)
   - Crea un `HttpClient` per fare richieste all'API

2. **Arrange**: Prepara dati di test (DTO, headers, etc.)

3. **Act**: Esegue richiesta HTTP reale all'endpoint
   - La richiesta passa attraverso tutti i middleware (rate limiting, authentication, authorization)
   - Viene eseguita la validazione FluentValidation
   - Viene eseguita la business logic
   - Viene generata la risposta HTTP

4. **Assert**: Verifica status code, corpo risposta, headers

**Esempio - Test Endpoint:**
- Input: POST `/readings` con DTO valido
- Operazione: Richiesta HTTP completa attraverso l'API
- Verifica: Status code 201 Created (o 401 se manca autenticazione)

### CustomWebApplicationFactory - Come Funziona

Il `CustomWebApplicationFactory` è un helper fondamentale per i test di integrazione:

1. **Eredita da `WebApplicationFactory<Program>`**: Usa il marker class `Program` per trovare l'applicazione

2. **Override `ConfigureWebHost`**: Personalizza la configurazione per i test
   - Rimuove il `DbContext` configurato per PostgreSQL
   - Aggiunge un `DbContext` configurato per database in-memory
   - Assicura che tutti i servizi necessari siano registrati (es. `AddAuthorization`)

3. **Risultato**: Un'applicazione completa e funzionante, ma con database in-memory invece di PostgreSQL

### Tecnologie Usate

- **xUnit**: Framework di testing per .NET
- **Microsoft.AspNetCore.Mvc.Testing**: Per creare istanze dell'applicazione per i test
- **Microsoft.EntityFrameworkCore.InMemory**: Database in-memory per test di integrazione
- **FluentValidation.TestHelper**: Helper per testare validatori FluentValidation

### Esecuzione dei Test

**Comando:**
```bash
dotnet test
```

**Output:**
- Compilazione del progetto principale e del progetto di test
- Esecuzione di tutti i test (unit + integration)
- Report con risultati: passati, falliti, ignorati
- Durata totale di esecuzione

### Stato Attuale dei Test

✅ **Test Unitari Implementati:**
- Validatore `CreateSensorReadingDtoValidator` (test limite valore)
- Servizio `JwtService` (generazione e validazione token)
- Servizio `TotpService` (generazione secret, validazione codici, QR code)

✅ **Test di Integrazione Implementati:**
- Test endpoint `POST /readings` (richiede autenticazione per completare)

⚠️ **Prossimi Passi per Test di Integrazione:**
- Configurare autenticazione nei test (creare utente, generare JWT token)
- Testare flussi completi con autenticazione
- Testare scenari di errore (validazione fallita, rate limiting, etc.)

### Percorsi di Navigazione - Testing

#### Voglio vedere come funzionano i test unitari

1. Test validatori → `tests/IndustrialSecureApi.Tests/Unit/Validators/`
2. Test servizi → `tests/IndustrialSecureApi.Tests/Unit/Services/`
3. Validatori testati → `src/IndustrialSecureApi/Features/*/Validators/`
4. Servizi testati → `src/IndustrialSecureApi/Features/*/Services/`

#### Voglio vedere come funzionano i test di integrazione

1. Test endpoint → `tests/IndustrialSecureApi.Tests/Integration/ApiTests.cs`
2. Factory per test → `tests/IndustrialSecureApi.Tests/Integration/TestHelpers/CustomWebApplicationFactory.cs`
3. Endpoint testati → `src/IndustrialSecureApi/program.cs` (cerca endpoint `/readings`)
4. Marker class → `src/IndustrialSecureApi/ProgramMarker.cs`

#### Voglio aggiungere un nuovo test

1. **Test unitario**: Crea file in `tests/IndustrialSecureApi.Tests/Unit/[Categoria]/`
2. **Test integrazione**: Aggiungi metodo in `tests/IndustrialSecureApi.Tests/Integration/ApiTests.cs` o crea nuovo file
3. **Usa `CustomWebApplicationFactory`**: Per test di integrazione, usa `IClassFixture<CustomWebApplicationFactory>`
4. **Esegui**: `dotnet test` dalla root o dalla cartella del progetto di test

---

## Containerizzazione e Orchestrazione

### Dockerfile Multi-Stage

Il progetto include un Dockerfile ottimizzato con build multi-stage per ridurre la dimensione dell'immagine finale.

**Struttura:**
- **Stage 1 (Build)**: Usa `mcr.microsoft.com/dotnet/sdk:8.0` per compilare l'applicazione
- **Stage 2 (Publish)**: Pubblica l'applicazione in modalità Release
- **Stage 3 (Runtime)**: Usa `mcr.microsoft.com/dotnet/aspnet:8.0-alpine` (immagine più piccola)

**Caratteristiche di Sicurezza:**
- User non-root con UID 1000 (`appuser`)
- Ownership corretta dei file
- Porta 8080 esposta
- Variabili d'ambiente configurate per produzione

**Percorso:** `src/IndustrialSecureApi/Dockerfile`

### Docker Compose

Il progetto include un `docker-compose.yml` completo che orchestra tutti i servizi necessari:

**Servizi Configurati:**

1. **app** (Industrial Secure API)
   - Build dal Dockerfile
   - Porta 8080 esposta
   - Dipende da postgres, redis, seq
   - Variabili d'ambiente per configurazione

2. **postgres** (PostgreSQL Database)
   - Immagine: `postgres:16-alpine`
   - Porta 5433 esposta (per evitare conflitti con PostgreSQL locale)
   - Health check configurato
   - Volume persistente per dati

3. **redis** (Token Blacklist)
   - Immagine: `redis:7-alpine`
   - Porta 6379 esposta
   - Persistenza abilitata (AOF)
   - Health check configurato

4. **seq** (Structured Logging)
   - Immagine: `datalust/seq:latest`
   - Porte 5341 (API) e 5342 (UI) esposte
   - Volume persistente per dati

**Caratteristiche:**
- Health checks su postgres e redis
- Restart policy `unless-stopped`
- Volumes persistenti per tutti i dati
- Dipendenze tra servizi configurate correttamente

**Percorso:** `docker-compose.yml` (root del repository)

### Come Usare Docker Compose

**Avviare tutti i servizi:**
```bash
docker-compose up -d
```

**Vedere i log:**
```bash
docker-compose logs -f app
```

**Fermare i servizi:**
```bash
docker-compose down
```

**Fermare e rimuovere volumi:**
```bash
docker-compose down -v
```

**Rebuild dell'applicazione:**
```bash
docker-compose up -d --build app
```

### Accesso ai Servizi

- **API**: `http://localhost:8080`
- **PostgreSQL**: `localhost:5433`
- **Redis**: `localhost:6379`
- **Seq UI**: `http://localhost:5342`
- **Seq API**: `http://localhost:5341`

---

## Dependency Scanning e Security

### Script di Audit (`scripts/audit.ps1`)

Il progetto include uno script PowerShell per il scanning automatico delle vulnerabilità nei pacchetti NuGet.

**Funzionalità:**
- Esegue `dotnet list package --vulnerable --include-transitive`
- Controlla vulnerabilità HIGH e CRITICAL
- **Fallisce il build** se trova vulnerabilità HIGH o CRITICAL (exit code 1)
- Avvisa (ma non blocca) per vulnerabilità Moderate o Low

**Come Funziona:**
1. Esegue il comando di scanning
2. Analizza l'output per trovare "HIGH" o "CRITICAL"
3. Se trova HIGH/CRITICAL: stampa errore e esce con codice 1 (fallisce build)
4. Se trova altre vulnerabilità: avvisa ma continua (exit code 0)
5. Se non trova vulnerabilità: successo (exit code 0)

**Esecuzione:**
```powershell
.\scripts\audit.ps1
```

**Integrazione CI/CD:**
Lo script può essere integrato in pipeline CI/CD per bloccare automaticamente il build se vengono trovate vulnerabilità critiche.

**Percorso:** `scripts/audit.ps1`

### Vulnerabilità Risolte

**Vulnerabilità HIGH Risolte:**
- ✅ `Npgsql 8.0.0` → CVE-2024-32655 (aggiornato a 8.0.3+)
- ✅ `Microsoft.Extensions.Caching.Memory 8.0.0` → DoS vulnerability (aggiornato a 8.0.2+)

**Vulnerabilità Moderate Rimanenti (Opzionali):**
- ⚠️ `Microsoft.IdentityModel.JsonWebTokens 7.0.3` → GHSA-59j7-ghrg-fj52
- ⚠️ `System.IdentityModel.Tokens.Jwt 7.0.3` → GHSA-59j7-ghrg-fj52

Le vulnerabilità Moderate non bloccano il build ma è consigliato risolverle aggiornando `Microsoft.AspNetCore.Authentication.JwtBearer` a 8.0.11+.

### Verifica Vulnerabilità

**Comando per verificare vulnerabilità:**
```bash
dotnet list package --vulnerable --include-transitive
```

**Comando per vedere pacchetti obsoleti:**
```bash
dotnet list package --outdated --include-transitive
```

### Percorsi di Navigazione - Docker & Security

#### Voglio vedere come funziona il Dockerfile

1. Dockerfile → `src/IndustrialSecureApi/Dockerfile`
2. Multi-stage build: Stage 1 (build), Stage 2 (publish), Stage 3 (runtime)
3. User non-root: cerca `adduser -D -u 1000`
4. Configurazione → variabili ENV e EXPOSE

#### Voglio vedere come funziona docker-compose

1. docker-compose.yml → `docker-compose.yml` (root)
2. Servizi: app, postgres, redis, seq
3. Dipendenze: cerca `depends_on`
4. Health checks: cerca `healthcheck`
5. Volumes: cerca sezione `volumes:`

#### Voglio eseguire lo script di audit

1. Script → `scripts/audit.ps1`
2. Esegui: `.\scripts\audit.ps1`
3. Output: mostra vulnerabilità HIGH/CRITICAL o Moderate
4. Build fallisce se trova HIGH/CRITICAL

---

## Infrastructure as Code (Terraform)

### Struttura Terraform

Il progetto include configurazione Terraform per creare l'infrastruttura Azure necessaria per l'applicazione in produzione.

**File Principali:**
- `infra/main.tf`: Provider Azure e configurazione base, Resource Group
- `infra/keyvault.tf`: Azure Key Vault con soft delete e purge protection
- `infra/secrets.tf`: Secrets nel Key Vault (connection string, JWT key, issuer, audience)
- `infra/managed-identity.tf`: User Assigned Managed Identity per accesso senza password
- `infra/variables.tf`: Variabili configurabili
- `infra/outputs.tf`: Output values (Key Vault URI, Managed Identity ID, etc.)
- `infra/terraform.tfvars.example`: Template di configurazione
- `infra/README-IaC.md`: Documentazione completa IaC

### Risorse Create

**Azure Key Vault:**
- Storage sicuro per tutti i secrets
- Soft delete abilitato (7 giorni retention)
- Purge protection abilitato
- Network ACLs configurate

**User Assigned Managed Identity:**
- Identità gestita per accesso senza password
- Può essere assegnata a Container Apps, App Service, AKS, etc.
- Access policy configurata per leggere secrets dal Key Vault

**Secrets:**
- `DatabaseConnectionString`: Connection string PostgreSQL
- `JwtKey`: Chiave segreta per JWT (minimo 32 caratteri)
- `JwtIssuer`: JWT Issuer
- `JwtAudience`: JWT Audience

### Come Funziona

**1. Setup Iniziale:**
- Copia `terraform.tfvars.example` come `terraform.tfvars`
- Inserisci i valori reali (NON committare `terraform.tfvars`)
- Esegui `terraform init` per inizializzare
- Esegui `terraform plan` per vedere cosa verrà creato
- Esegui `terraform apply` per creare le risorse

**2. Managed Identity:**
- La Managed Identity viene creata automaticamente
- Viene configurata un access policy sul Key Vault per permettere alla Managed Identity di leggere secrets
- L'applicazione può usare `DefaultAzureCredential` per accedere al Key Vault senza password

**3. Accesso da .NET:**
- L'applicazione usa `Azure.Identity.DefaultAzureCredential`
- Automaticamente rileva la Managed Identity quando deployata su Azure
- Accede al Key Vault usando l'identità, senza necessità di connection strings o password hardcoded

### Flussi di Esecuzione

#### Creazione Infrastruttura

1. **Preparazione**: Copia `terraform.tfvars.example` e configura valori
2. **Inizializzazione**: `terraform init` scarica provider Azure
3. **Pianificazione**: `terraform plan` mostra cosa verrà creato
4. **Applicazione**: `terraform apply` crea le risorse Azure
5. **Output**: Terraform mostra URI Key Vault e Managed Identity ID

#### Utilizzo in Produzione

1. **Deploy Applicazione**: Assegna Managed Identity all'app (Container Apps, App Service, etc.)
2. **Configurazione**: L'app usa `DefaultAzureCredential` per accedere al Key Vault
3. **Accesso Secrets**: L'app legge secrets dal Key Vault usando Managed Identity
4. **Nessuna Password**: Nessuna password o connection string hardcoded nel codice

### Sicurezza

**Best Practices Implementate:**
- Soft delete su Key Vault (7 giorni retention)
- Purge protection per prevenire eliminazione accidentale
- Network ACLs configurate (default deny, bypass per Azure Services)
- Secrets marcati come `sensitive` in Terraform
- `terraform.tfvars` escluso da Git (non committato)
- Managed Identity invece di password hardcoded

### Gestione Secrets

**Aggiungere Nuovo Secret:**
1. Aggiungi risorsa in `secrets.tf`
2. Aggiungi variabile in `variables.tf`
3. Aggiungi valore in `terraform.tfvars`
4. Esegui `terraform apply`

**Aggiornare Secret:**
1. Modifica valore in `terraform.tfvars`
2. Esegui `terraform apply`

**Rimuovere Secret:**
1. Rimuovi risorsa da `secrets.tf`
2. Esegui `terraform apply`

### Integrazione CI/CD

Il job `infrastructure-plan` in `.github/workflows/ci.yml` esegue `terraform plan` su ogni Pull Request per verificare le modifiche all'infrastruttura senza applicarle.

### Percorsi di Navigazione - Infrastructure as Code

#### Voglio vedere la configurazione Terraform

1. File principali → `infra/` (root del repository)
2. Key Vault → `infra/keyvault.tf`
3. Secrets → `infra/secrets.tf`
4. Managed Identity → `infra/managed-identity.tf`
5. Variabili → `infra/variables.tf`
6. Output → `infra/outputs.tf`

#### Voglio creare/modificare l'infrastruttura

1. Configurazione → `infra/terraform.tfvars` (non committato)
2. Template → `infra/terraform.tfvars.example`
3. Documentazione → `infra/README-IaC.md`
4. Comandi: `terraform init`, `terraform plan`, `terraform apply`

#### Voglio vedere come l'app accede ai secrets

1. Configurazione Key Vault → `infra/keyvault.tf`
2. Access policy → cerca `azurerm_key_vault_access_policy` in `keyvault.tf`
3. Managed Identity → `infra/managed-identity.tf`
4. Utilizzo in .NET → cerca `DefaultAzureCredential` e `AddAzureKeyVault` in `program.cs` (da implementare)

---

## API Documentation (Swagger/OpenAPI)

### Configurazione Swagger

Il progetto include Swagger/OpenAPI con documentazione completa degli endpoint.

**Configurazione:**
- Swagger configurato in `program.cs` con `AddSwaggerGen`
- Include documentazione XML dai commenti
- Configurazione JWT Bearer Authentication per testare endpoint protetti
- Swagger UI disponibile solo in ambiente Development

**Accesso:**
- Swagger UI: `http://localhost:5000` o `https://localhost:5001` (in Development)
- OpenAPI JSON: `http://localhost:5000/swagger/v1/swagger.json`

**Caratteristiche:**
- Documentazione automatica di tutti gli endpoint
- Esempi di request/response
- Test interattivo degli endpoint direttamente dal browser
- Supporto per autenticazione JWT Bearer (inserisci token nel campo Authorization)

**Documentazione Endpoint:**
Gli endpoint sono documentati usando i metodi di estensione delle Minimal API:
- `.WithSummary()`: Breve descrizione dell'endpoint
- `.WithDescription()`: Descrizione dettagliata
- `.WithTags()`: Categorizzazione endpoint
- `.Produces()`: Codici di risposta possibili

**Percorso:** Configurazione in `program.cs` (cerca `AddSwaggerGen` e `UseSwagger`)

---

## Security Controls - OWASP Top 10 Mapping

### A01:2021 - Broken Access Control

**Implementato:**
- RBAC con ruoli Operator e Manager
- JWT Bearer Authentication con validazione token
- Endpoint protetti con `RequireAuthorization`
- Refresh token con revoca
- Policy di autorizzazione basate su ruoli

**Dove:**
- Configurazione ruoli → `Infrastructure/Seeders/DataSeeder.cs`
- Autorizzazione endpoint → `program.cs` (cerca `RequireAuthorization`)
- JWT validation → `program.cs` (cerca `AddJwtBearer`)

### A02:2021 - Cryptographic Failures

**Implementato:**
- Password hashing con ASP.NET Core Identity (PBKDF2)
- JWT con chiave segreta configurata (minimo 32 caratteri)
- Secrets in Azure Key Vault (non hardcoded)
- HTTPS enforcement (in produzione)
- Managed Identity per accesso senza password

**Dove:**
- Password hashing → ASP.NET Core Identity (automatico)
- JWT configuration → `appsettings.json` e `program.cs`
- Key Vault → `infra/keyvault.tf` e `infra/secrets.tf`

### A03:2021 - Injection

**Implementato:**
- EF Core con parameterized queries (previene SQL injection)
- FluentValidation per input validation
- Row-Level Security su PostgreSQL per audit trail
- Validazione tipo-safe con C# strong typing

**Dove:**
- EF Core → `Infrastructure/Data/ApplicationDbContext.cs`
- Validazione input → `Features/*/Validators/`
- RLS → Migrations SQL

### A04:2021 - Insecure Design

**Implementato:**
- Audit trail immutabile (Row-Level Security)
- Rate limiting per prevenire abusi
- 2FA con TOTP
- Separazione concerns (Features-based architecture)
- Dependency scanning automatico

**Dove:**
- Audit trail → `ApplicationDbContext.SaveChangesAsync` override
- Rate limiting → `program.cs` (cerca `UseIpRateLimiting`)
- 2FA → `Features/Auth/Services/Implementations/TotpService.cs`

### A05:2021 - Security Misconfiguration

**Implementato:**
- User non-root nel container Docker
- Secrets management con Azure Key Vault
- Managed Identity (no password hardcoded)
- Dependency scanning automatico
- Environment-specific configuration

**Dove:**
- Docker user → `src/IndustrialSecureApi/Dockerfile`
- Key Vault → `infra/` (Terraform)
- Dependency scan → `scripts/audit.ps1`

### A06:2021 - Vulnerable and Outdated Components

**Implementato:**
- Dependency audit script (blocca build se HIGH)
- Dependabot per aggiornamenti automatici
- Trivy scan per vulnerabilità container
- CodeQL per SAST
- Monitoring continuo vulnerabilità

**Dove:**
- Dependency audit → `scripts/audit.ps1`
- CI/CD scans → `.github/workflows/trivy.yml` e `.github/workflows/codeql.yml`
- Dependabot → `.github/dependabot.yml`

### A07:2021 - Identification and Authentication Failures

**Implementato:**
- ASP.NET Core Identity con password policy
- JWT con expiration e refresh tokens
- TOTP per 2FA
- Account lockout su tentativi falliti
- Password requirements configurate

**Dove:**
- Identity configuration → `program.cs` (cerca `AddIdentity`)
- JWT → `Features/Auth/Services/Implementations/JwtService.cs`
- TOTP → `Features/Auth/Services/Implementations/TotpService.cs`

### A08:2021 - Software and Data Integrity Failures

**Implementato:**
- CI/CD pipeline con test automatici
- Immutabilità audit trail (Row-Level Security)
- Versioning semantico (CHANGELOG.md)
- State management Terraform

**Dove:**
- CI/CD → `.github/workflows/ci.yml`
- Audit immutability → PostgreSQL RLS policies
- Versioning → `CHANGELOG.md`

### A09:2021 - Security Logging and Monitoring Failures

**Implementato:**
- Serilog con logging strutturato
- Error logging middleware (4xx/5xx)
- Audit trail completo di tutte le operazioni
- Logging su Seq per analisi
- Logging su file con rotazione

**Dove:**
- Serilog config → `program.cs` (inizio file)
- Error middleware → `Infrastructure/Middleware/ErrorLoggingMiddleware.cs`
- Audit trail → `ApplicationDbContext.SaveChangesAsync`

### A10:2021 - Server-Side Request Forgery (SSRF)

**Implementato:**
- Validazione input con FluentValidation
- Rate limiting per prevenire abusi
- Nessuna funzionalità che accetta URL esterni
- Validazione tipo-safe

**Dove:**
- Input validation → `Features/*/Validators/`
- Rate limiting → `program.cs` (cerca `UseIpRateLimiting`)

---

## Changelog

Il progetto include un `CHANGELOG.md` che documenta tutti i cambiamenti notevoli seguendo il formato [Keep a Changelog](https://keepachangelog.com/) e [Semantic Versioning](https://semver.org/).

**Formato:**
- `Added`: Nuove funzionalità
- `Changed`: Modifiche a funzionalità esistenti
- `Deprecated`: Funzionalità che verranno rimosse
- `Removed`: Funzionalità rimosse
- `Fixed`: Bug fixes
- `Security`: Vulnerabilità risolte

**Percorso:** `CHANGELOG.md` (root del repository)

**Versione Corrente:** 1.2.0 (Release Workflow Implemented)

---

## Prossimi Sviluppi

### Endpoint da Completare
- `POST /readings`: ✅ Implementato con validazione FluentValidation
- `GET /readings`: Implementare lista letture con filtri
- `GET /readings/{id}`: Implementare dettaglio lettura
- `DELETE /readings/{id}`: Implementare logica di eliminazione

### Flussi da Implementare
- Registrazione utente (`POST /api/auth/register`)
- Login completo (`POST /api/auth/login`) con password + 2FA + JWT

### Servizi da Aggiungere
- SensorService: Logica business per gestione sensori
- Health checks: Endpoint per monitoraggio stato applicazione

### Middleware da Aggiungere
- Request/Response logging completo (ora solo errori)
- Error handling centralizzato con pagine di errore personalizzate
- Correlation ID per tracciare richieste attraverso i log

### Validatori da Aggiungere
- Validatori per altri DTO (RegisterDto, LoginDto, etc.)

### Test da Completare
- Test di integrazione con autenticazione (creare utente, generare JWT, testare endpoint protetti)
- Test scenari di errore (validazione fallita, rate limiting, autorizzazione negata)
- Test per tutti gli endpoint implementati
- Test per middleware (error logging, rate limiting)
- Test per audit trail (verificare che vengano creati record di audit)

### Docker & Infrastructure da Completare
- ✅ Dockerfile multi-stage implementato
- ✅ docker-compose.yml con tutti i servizi
- ✅ Script dependency scanning implementato
- ⚠️ Integrare script audit in CI/CD pipeline
- ⚠️ Configurare Redis per token blacklist (implementazione)
- ⚠️ Testare build e deploy con Docker

### Security da Completare
- ✅ Vulnerabilità HIGH risolte
- ✅ Infrastructure as Code con Terraform implementato
- ✅ Azure Key Vault e Managed Identity configurati
- ⚠️ Risolvere vulnerabilità Moderate (JWT packages)
- ⚠️ Implementare token blacklist con Redis
- ⚠️ Integrare Azure Key Vault in applicazione .NET (DefaultAzureCredential)

### Infrastructure da Completare
- ✅ Terraform configuration implementata
- ✅ Azure Key Vault e Managed Identity definiti
- ⚠️ Integrare Key Vault in applicazione (AddAzureKeyVault in program.cs)
- ⚠️ Configurare Azure Container Apps con Managed Identity
- ⚠️ Testare deploy completo su Azure

### Documentazione da Completare
- ✅ Swagger/OpenAPI implementato
- ✅ Security Controls (OWASP Top 10) documentati
- ✅ CHANGELOG.md creato
- ✅ Release workflow implementato
- ✅ SBOM generation (CycloneDX) implementato
- ⚠️ Aggiungere più esempi di request/response in Swagger
- ⚠️ Documentare tutti gli endpoint con commenti XML

### Release da Completare
- ✅ Release workflow GitHub Actions implementato
- ✅ SBOM generation con CycloneDX
- ✅ Docker image publishing su GHCR
- ⚠️ Testare processo completo di release
- ⚠️ Configurare automatic release notes da CHANGELOG

---

**Ultimo aggiornamento:** Dicembre 2024  
**Versione:** 1.2.0 (Release Workflow Implemented)
