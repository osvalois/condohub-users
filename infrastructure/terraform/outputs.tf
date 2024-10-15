output "resource_group_name" {
  value = azurerm_resource_group.rg.name
}

output "sql_server_name" {
  value = azurerm_sql_server.sql_server.name
}

output "sql_database_name" {
  value = azurerm_sql_database.sql_db.name
}

output "app_service_name" {
  value = azurerm_app_service.app_service.name
}

output "app_service_default_hostname" {
  value = azurerm_app_service.app_service.default_site_hostname
}

output "aad_application_id" {
  value = azuread_application.aad_app.application_id
}

output "key_vault_uri" {
  value = azurerm_key_vault.key_vault.vault_uri
}

output "app_insights_instrumentation_key" {
  value     = azurerm_application_insights.app_insights.instrumentation_key
  sensitive = true
}