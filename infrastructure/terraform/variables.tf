variable "resource_group_name" {
  description = "The name of the resource group"
  type        = string
}

variable "location" {
  description = "The Azure region where resources will be created"
  type        = string
  default     = "East US"
}

variable "sql_server_name" {
  description = "The name of the SQL Server"
  type        = string
}

variable "sql_database_name" {
  description = "The name of the SQL Database"
  type        = string
}

variable "sql_admin_login" {
  description = "The administrator login for the SQL Server"
  type        = string
}

variable "sql_admin_password" {
  description = "The administrator password for the SQL Server"
  type        = string
}

variable "app_service_plan_name" {
  description = "The name of the App Service Plan"
  type        = string
}

variable "app_service_name" {
  description = "The name of the App Service"
  type        = string
}

variable "environment" {
  description = "The environment (dev, staging, prod)"
  type        = string
}

variable "aad_app_name" {
  description = "The name of the Azure AD Application"
  type        = string
}

variable "key_vault_name" {
  description = "The name of the Azure Key Vault"
  type        = string
}

variable "app_insights_name" {
  description = "The name of the Application Insights resource"
  type        = string
}