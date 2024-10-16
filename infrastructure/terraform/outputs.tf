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
output "apim_gateway_url" {
  value = azurerm_api_management.apim.gateway_url
}

output "acr_login_server" {
  value = azurerm_container_registry.acr.login_server
}

output "log_analytics_workspace_id" {
  value = azurerm_log_analytics_workspace.law.id
}