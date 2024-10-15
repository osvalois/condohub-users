# Azure Resource Group
resource "azurerm_resource_group" "rg" {
  name     = var.resource_group_name
  location = var.location
}

# Azure SQL Server
resource "azurerm_sql_server" "sql_server" {
  name                         = var.sql_server_name
  resource_group_name          = azurerm_resource_group.rg.name
  location                     = azurerm_resource_group.rg.location
  version                      = "12.0"
  administrator_login          = var.sql_admin_login
  administrator_login_password = var.sql_admin_password

  tags = {
    environment = var.environment
  }
}

# Azure SQL Database
resource "azurerm_sql_database" "sql_db" {
  name                = var.sql_database_name
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  server_name         = azurerm_sql_server.sql_server.name

  tags = {
    environment = var.environment
  }
}

# Azure App Service Plan
resource "azurerm_app_service_plan" "app_plan" {
  name                = var.app_service_plan_name
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name

  sku {
    tier = "Standard"
    size = "S1"
  }
}

# Azure App Service
resource "azurerm_app_service" "app_service" {
  name                = var.app_service_name
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  app_service_plan_id = azurerm_app_service_plan.app_plan.id

  site_config {
    dotnet_framework_version = "v6.0"
    scm_type                 = "LocalGit"
  }

  app_settings = {
    "WEBSITE_NODE_DEFAULT_VERSION" = "10.14.1"
    "ASPNETCORE_ENVIRONMENT"       = var.environment
  }

  connection_string {
    name  = "DefaultConnection"
    type  = "SQLAzure"
    value = "Server=tcp:${azurerm_sql_server.sql_server.fully_qualified_domain_name},1433;Initial Catalog=${azurerm_sql_database.sql_db.name};Persist Security Info=False;User ID=${var.sql_admin_login};Password=${var.sql_admin_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  }
}

# Azure Active Directory App Registration
resource "azuread_application" "aad_app" {
  display_name = var.aad_app_name
}

# Azure Active Directory App Service Principal
resource "azuread_service_principal" "aad_sp" {
  application_id = azuread_application.aad_app.application_id
}

# Azure Key Vault
resource "azurerm_key_vault" "key_vault" {
  name                        = var.key_vault_name
  location                    = azurerm_resource_group.rg.location
  resource_group_name         = azurerm_resource_group.rg.name
  enabled_for_disk_encryption = true
  tenant_id                   = data.azurerm_client_config.current.tenant_id
  soft_delete_retention_days  = 7
  purge_protection_enabled    = false

  sku_name = "standard"

  access_policy {
    tenant_id = data.azurerm_client_config.current.tenant_id
    object_id = data.azurerm_client_config.current.object_id

    key_permissions = [
      "Get",
    ]

    secret_permissions = [
      "Get", "Set", "List", "Delete", "Purge"
    ]

    storage_permissions = [
      "Get",
    ]
  }
}

# Azure Application Insights
resource "azurerm_application_insights" "app_insights" {
  name                = var.app_insights_name
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  application_type    = "web"
}

data "azurerm_client_config" "current" {}