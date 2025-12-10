variable "resource_group_name" {
  description = "Nome del Resource Group Azure"
  type        = string
}

variable "location" {
  description = "Regione Azure"
  type        = string
  default     = "West Europe"
}

variable "key_vault_name" {
  description = "Nome del Key Vault (deve essere unico globalmente)"
  type        = string
}

variable "managed_identity_name" {
  description = "Nome della Managed Identity"
  type        = string
  default     = "industrial-secure-api-identity"
}

variable "database_connection_string" {
  description = "Connection string del database PostgreSQL"
  type        = string
  sensitive   = true
}

variable "jwt_key" {
  description = "Chiave segreta per JWT (minimo 32 caratteri)"
  type        = string
  sensitive   = true
}

variable "jwt_issuer" {
  description = "JWT Issuer"
  type        = string
  default     = "IndustrialSecureAPI"
}

variable "jwt_audience" {
  description = "JWT Audience"
  type        = string
  default     = "IndustrialSecureAPI"
}

variable "tags" {
  description = "Tag da applicare alle risorse"
  type        = map(string)
  default = {
    Environment = "Production"
    Project     = "IndustrialSecureAPI"
    ManagedBy   = "Terraform"
  }
}