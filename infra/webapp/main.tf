#----------------------------------------------------------------------------
#  -App Service Plan and App Service  
/*
resource "azurerm_app_service_plan" "app-plan" {
  name                = "${var.NAME}-plan"
  location            = var.LOCATION
  resource_group_name = var.APP_RG_NAME

  #kind                = "linux"  ??
  #reserved            = true ??

  sku {
    tier = "Standard"
    size = "S1"
  }
}

resource "azurerm_app_service" "web-app" {


 depends_on = [
    var.DB_CREATION_DONE,
    azurerm_application_insights.svc-ppl
  ]
  

  name               = var.NAME
  location            = var.LOCATION
  resource_group_name = var.APP_RG_NAME
  https_only          = false
  app_service_plan_id = azurerm_app_service_plan.app-plan.id

  site_config {
    always_on                 = "true"
    app_command_line          = ""
    linux_fx_version          = "DOCKER|${var.NAME}.azurecr.io/${var.REPO}:latest"
    use_32_bit_worker_process = "true"
  }
  
  identity {
     type = "SystemAssigned"
   } 

  logs {
    http_logs {
      file_system {
        retention_in_days = 30
        retention_in_mb   = 100
      }
    }
  } 

  app_settings = {
    "WEBSITES_ENABLE_APP_SERVICE_STORAGE" = "true"
    "WEBSITES_ENABLE_APP_SERVICE_STORAGE" = "true"
    "KEYVAULT_NAME"                       = "${var.NAME}-kv"
    "APPINSIGHTS_INSTRUMENTATIONKEY" = "${azurerm_application_insights.svc-ppl-appi.instrumentation_key}"
  
  }
  tags = {
    environment = "development"
  }

}


output "APP_SERVICE_DONE" {
  depends_on  = [azurerm_app_service.web-app]
  value       = true
  description = "App Service setup is complete"
}
*/
#---------------------------------------------------------------------------

