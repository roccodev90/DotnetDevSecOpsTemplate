# Infrastructure as Code - Terraform

Questo modulo Terraform crea l'infrastruttura Azure necessaria per Industrial Secure API.

## Risorse Create

- **Azure Key Vault**: Storage sicuro per secrets
- **User Assigned Managed Identity**: Identità gestita per accesso senza password
- **Secrets**: Connection string database, JWT key, issuer, audience

## Prerequisiti

1. **Azure CLI** installato e configurato
2. **Terraform** >= 1.6.0 installato
3. **Account Azure** con permessi per creare risorse
4. **Azure Subscription** attiva

## Setup Iniziale

### 1. Login Azure
h
az login
az account set --subscription "YOUR_SUBSCRIPTION_ID"### 2. Configura Variabili

Copia il file di esempio e inserisci i valori reali:

cp terraform.tfvars.example terraform.tfvars**IMPORTANTE**: Il file `terraform.tfvars` è nel `.gitignore` e NON deve essere committato.

Modifica `terraform.tfvars` con i tuoi valori:

resource_group_name = "rg-industrial-secure-api-prod"
key_vault_name      = "kv-industrial-secure-api-prod-001"  # Deve essere unico globalmente
database_connection_string = "Host=..."  # Connection string reale
jwt_key = "YourRealSecretKey..."  # Chiave JWT reale
### 3. Inizializza Terraform

cd infra
terraform init### 4. Verifica Piano

terraform planQuesto mostra cosa verrà creato senza applicare le modifiche.

### 5. Applica Configurazione

terraform applyTerraform chiederà conferma. Digita `yes` per procedere.

## Utilizzo Managed Identity

La Managed Identity creata può essere assegnata a:

- **Azure Container Apps** (per l'applicazione)
- **Azure App Service**
- **Azure Functions**
- **Azure Kubernetes Service**

L'applicazione può accedere al Key Vault usando la Managed Identity senza dover gestire password o connection strings hardcoded.

### Esempio: Configurazione in Azure Container Apps

identity:
  type: UserAssigned
  userAssignedIdentities:
    - resourceId: /subscriptions/.../resourceGroups/.../providers/Microsoft.ManagedIdentity/userAssignedIdentities/mi-industrial-secure-api-prod### Esempio: Accesso da .NET

// In program.cs o Startup.cs
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential()  // Usa automaticamente Managed Identity
);## Gestione Secrets

### Aggiungere un Nuovo Secret

1. Aggiungi la risorsa in `secrets.tf`:

resource "azurerm_key_vault_secret" "new_secret" {
  name         = "NewSecretName"
  value        = var.new_secret_value
  key_vault_id = azurerm_key_vault.main.id
  content_type = "text/plain"
}2. Aggiungi la variabile in `variables.tf`:
cl
variable "new_secret_value" {
  description = "Descrizione del secret"
  type        = string
  sensitive   = true
}3. Aggiungi il valore in `terraform.tfvars`

4. Esegui `terraform apply`

### Aggiornare un Secret

Modifica il valore in `terraform.tfvars` ed esegui:
sh
terraform applyTerraform aggiornerà il secret nel Key Vault.

### Rimuovere un Secret

Rimuovi la risorsa da `secrets.tf` ed esegui:

terraform apply## Output

Dopo `terraform apply`, vengono mostrati gli output:

- `key_vault_name`: Nome del Key Vault
- `key_vault_uri`: URI completo del Key Vault
- `managed_identity_client_id`: Client ID della Managed Identity
- `managed_identity_principal_id`: Principal ID della Managed Identity
- `managed_identity_id`: ID completo della Managed Identity

Per vedere gli output in qualsiasi momento:

terraform output## Distruzione Risorse

**ATTENZIONE**: Questo elimina tutte le risorse create, inclusi i secrets nel Key Vault.
h
terraform destroy## Best Practices

1. **Non committare `terraform.tfvars`**: Contiene secrets sensibili
2. **Usa backend remoto**: Per state file condiviso (opzionale, configura in `main.tf`)
3. **Versioning**: Usa versioning per il codice Terraform
4. **Tag**: Applica tag consistenti alle risorse
5. **Soft Delete**: Key Vault ha soft delete abilitato (7 giorni retention)
6. **Purge Protection**: Abilitato per prevenire eliminazione accidentale

## Troubleshooting

### Errore: Key Vault name già in uso

Il nome del Key Vault deve essere unico globalmente. Cambia `key_vault_name` in `terraform.tfvars`.

### Errore: Permessi insufficienti

Assicurati di avere i permessi necessari:
h
az role assignment list --assignee $(az account show --query user.name -o tsv) --scope /subscriptions/YOUR_SUBSCRIPTION_ID### Errore: Managed Identity non può accedere al Key Vault

Verifica che l'access policy sia stata creata correttamente:

az keyvault show --name YOUR_KEY_VAULT_NAME --query properties.accessPolicies## Integrazione CI/CD

Il job `infrastructure-plan` in `.github/workflows/ci.yml` esegue `terraform plan` su ogni Pull Request per verificare le modifiche all'infrastruttura.

## Riferimenti

- [Azure Key Vault Documentation](https://docs.microsoft.com/azure/key-vault/)
- [Managed Identity Documentation](https://docs.microsoft.com/azure/active-directory/managed-identities-azure-resources/)
- [Terraform Azure Provider](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs)