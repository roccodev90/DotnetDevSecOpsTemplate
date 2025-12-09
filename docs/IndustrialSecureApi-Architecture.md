# Industrial Secure API - Mappa di Navigazione del Codice

**Versione:** 0.7.0  
**Stato:** Logging & Audit Implemented  
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
└── program.cs                   # Entry point e configurazione
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
- Registra i servizi custom (JwtService, TotpService)
- Definisce tutti gli endpoint API
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

---

**Ultimo aggiornamento:** Dicembre 2024  
**Versione:** 0.7.0 (Logging & Audit Implemented)
