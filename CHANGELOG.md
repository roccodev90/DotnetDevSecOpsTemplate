# Changelog

Tutti i cambiamenti notevoli a questo progetto saranno documentati in questo file.

Il formato è basato su [Keep a Changelog](https://keepachangelog.com/it/1.0.0/),
e questo progetto aderisce a [Semantic Versioning](https://semver.org/lang/it/).

## [1.0.0] - 2024-12-09

### Added
- Infrastructure as Code con Terraform
- Azure Key Vault e Managed Identity per secrets management
- Docker multi-stage build con user non-root
- Docker Compose con orchestrazione completa (app, postgres, redis, seq)
- Dependency scanning script (blocca build se vulnerabilità HIGH)
- CI/CD pipeline GitHub Actions completa
- Test unitari e di integrazione
- Swagger/OpenAPI documentation con XML comments
- Security Controls mapping OWASP Top 10

### Security
- Risolte vulnerabilità HIGH: Npgsql 8.0.0, Microsoft.Extensions.Caching.Memory 8.0.0
- Dependency audit automatico in CI/CD
- Trivy e CodeQL scansioni sicurezza

### Changed
- Aggiornato Npgsql a 8.0.11 per risolvere vulnerabilità
- Aggiornato Microsoft.Extensions.Caching.Memory per risolvere DoS vulnerability

## [0.9.0] - 2024-12-09

### Added
- Docker containerization
- Dependency scanning infrastructure

## [0.8.0] - 2024-12-09

### Added
- Testing infrastructure (unit e integration tests)
- CustomWebApplicationFactory per test in-memory

## [0.7.0] - 2024-12-09

### Added
- Logging strutturato con Serilog
- Error logging middleware
- Rate limiting con AspNetCoreRateLimit

## [0.6.0] - 2024-12-09

### Added
- Input validation con FluentValidation
- JWT Bearer Authentication
- Refresh tokens con revoca

## [0.5.0] - 2024-12-09

### Added
- TOTP per 2FA
- RBAC con ruoli Operator e Manager
- Audit trail immutabile con Row-Level Security

## [0.1.0] - 2024-12-09

### Added
- Struttura progetto iniziale
- Database PostgreSQL con EF Core
- ASP.NET Core Identity
- Modelli sensori e audit

[1.0.0]: https://github.com/roccodev90/DotnetDevSecOpsTemplate/releases/tag/v1.0.0
[0.9.0]: https://github.com/roccodev90/DotnetDevSecOpsTemplate/releases/tag/v0.9.0
[0.8.0]: https://github.com/roccodev90/DotnetDevSecOpsTemplate/releases/tag/v0.8.0
[0.7.0]: https://github.com/roccodev90/DotnetDevSecOpsTemplate/releases/tag/v0.7.0
[0.6.0]: https://github.com/roccodev90/DotnetDevSecOpsTemplate/releases/tag/v0.6.0
[0.5.0]: https://github.com/roccodev90/DotnetDevSecOpsTemplate/releases/tag/v0.5.0
[0.1.0]: https://github.com/roccodev90/DotnetDevSecOpsTemplate/releases/tag/v0.1.0