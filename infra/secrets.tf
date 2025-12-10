# Secret: Database Connection String
resource "azurerm_key_vault_secret" "db_connection_string" {
  name         = "DatabaseConnectionString"
  value        = var.database_connection_string
  key_vault_id = azurerm_key_vault.main.id

  content_type = "text/plain"

  depends_on = [
    azurerm_key_vault_access_policy.current_user
  ]

  tags = var.tags
}

# Secret: JWT Key
resource "azurerm_key_vault_secret" "jwt_key" {
  name         = "JwtKey"
  value        = var.jwt_key
  key_vault_id = azurerm_key_vault.main.id

  content_type = "text/plain"

  depends_on = [
    azurerm_key_vault_access_policy.current_user
  ]

  tags = var.tags
}

# Secret: JWT Issuer
resource "azurerm_key_vault_secret" "jwt_issuer" {
  name         = "JwtIssuer"
  value        = var.jwt_issuer
  key_vault_id = azurerm_key_vault.main.id

  content_type = "text/plain"

  depends_on = [
    azurerm_key_vault_access_policy.current_user
  ]

  tags = var.tags
}

# Secret: JWT Audience
resource "azurerm_key_vault_secret" "jwt_audience" {
  name         = "JwtAudience"
  value        = var.jwt_audience
  key_vault_id = azurerm_key_vault.main.id

  content_type = "text/plain"

  depends_on = [
    azurerm_key_vault_access_policy.current_user
  ]

  tags = var.tags
}