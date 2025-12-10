# Resource Group
resource_group_name = "rg-industrial-secure-api-prod"
location            = "West Europe"

# Key Vault (nome deve essere unico globalmente, solo minuscole e numeri)
key_vault_name = "kv-industrial-secure-api-prod-001"

# Managed Identity
managed_identity_name = "mi-industrial-secure-api-prod"

# Secrets (NON committare questo file con valori reali!)
# Copia questo file come terraform.tfvars e inserisci i valori reali
database_connection_string = "Host=your-postgres-server.postgres.database.azure.com;Port=5432;Database=industrial_secure;Username=youruser@yourserver;Password=YOUR_PASSWORD;Ssl Mode=Require;"
jwt_key                   = "YourSuperSecretKeyThatIsAtLeast32CharactersLong!"
jwt_issuer                = "IndustrialSecureAPI"
jwt_audience              = "IndustrialSecureAPI"

# Tags
tags = {
  Environment = "Production"
  Project     = "IndustrialSecureAPI"
  ManagedBy   = "Terraform"
  Owner       = "DevOps Team"
}