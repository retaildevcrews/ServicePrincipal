resource azurerm_application_insights instance {
  name                = "ai-${var.PROJECT_NAME}-${var.ENV}"
  location            = var.LOCATION
  resource_group_name = var.APP_RG_NAME
  application_type    = "web"
}

output "instrumentation_key" {
  depends_on  = [azurerm_application_insights.instance]
  value = azurerm_application_insights.instance.instrumentation_key
}
