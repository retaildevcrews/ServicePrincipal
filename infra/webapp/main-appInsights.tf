resource azurerm_application_insights svc-ppl-appi {
  name                = "${var.NAME}-appi-${var.ENV}"
  location            = var.LOCATION
  resource_group_name = var.APP_RG_NAME
  application_type    = "web"
}


output "instrumentation_key" {
  depends_on  = [azurerm_application_insights.svc-ppl-appi]
  value = azurerm_application_insights.svc-ppl-appi.instrumentation_key
}
/*
output "app_id" {
  value = azurerm_application_insights.svc-ppl-appi.app_id
}
*/