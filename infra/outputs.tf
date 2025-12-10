output "key_vault_name" {
  description = "Nome del Key Vault creato"
  value       = azurerm_key_vault.main.name
}

output "key_vault_uri" {
  description = "URI del Key Vault"
  value       = azurerm_key_vault.main.vault_uri
}

output "managed_identity_client_id" {
  description = "Client ID della Managed Identity"
  value       = azurerm_user_assigned_identity.main.client_id
}

output "managed_identity_principal_id" {
  description = "Principal ID della Managed Identity"
  value       = azurerm_user_assigned_identity.main.principal_id
}

output "managed_identity_id" {
  description = "ID completo della Managed Identity"
  value       = azurerm_user_assigned_identity.main.id
}