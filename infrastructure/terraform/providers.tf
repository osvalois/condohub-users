terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 2.0"
    }
    azuread = {
      source  = "hashicorp/azuread"
      version = "~> 1.0"
    }
  }
}

provider "azurerm" {
  features {}
}

provider "azuread" {
  # No additional configuration needed if using Azure CLI authentication
}