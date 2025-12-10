terraform {
  required_version = ">= 1.6.0"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
  }

  backend "azurerm" {
    # Configurazione backend (opzionale, per state remoto)
    # resource_group_name  = "terraform-state-rg"
    # storage_account_name = "terraformstate"
    # container_name       = "tfstate"
    # key                  = "industrial-secure-api.terraform.tfstate"
  }
}

provider "azurerm" {
  features {}
}

# Resource Group
resource "azurerm_resource_group" "main" {
  name     = var.resource_group_name
  location = var.location

  tags = var.tags
}