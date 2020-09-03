resource azurerm_application_insights svc_ppl {
  name                = var.NAME
  location            = var.LOCATION
  resource_group_name = var.APP_RG_NAME
  application_type    = "web"
}


output "instrumentation_key" {
  value = azurerm_application_insights.svc_ppl.instrumentation_key
}
/*
output "app_id" {
  value = azurerm_application_insights.svc_ppl.app_id
}
*/