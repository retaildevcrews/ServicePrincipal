# NOTE: Storage account name can only consist of lowercase letters and numbers, and must be between 3 and 24 characters long

# resource azurerm_storage_account svc-ppl-storage-acc {
#   name                      = "${var.PROJECT_NAME}st${var.ENV}"
#   location                  = var.LOCATION
#   resource_group_name       = var.APP_RG_NAME
#   account_tier              = "Standard"
#   account_replication_type  = "LRS"
# }


data "azurerm_storage_account" "svc-ppl-storage-acc" {
  name                          = var.STORAGE_NAME
  resource_group_name             = var.APP_RG_NAME
}


output "STORAGE_ACCOUNT_DONE" {
  depends_on  = [  data.azurerm_storage_account.svc-ppl-storage-acc
          ]
  value       = true
  description = "Storage Account setup is complete"
}

# output "STORAGE_ACCOUNT_NAME" {
#   depends_on  = [
#     data.azurerm_storage_account.svc-ppl-storage-acc
#    ] 
#   value       = locals.storage_acc_name
#   description = "Storage Account name"
# }

resource "azurerm_app_service_plan" "app-plan" {
    name                = "${var.NAME}-plan-${var.ENV}"
    location            = var.LOCATION
    resource_group_name = var.APP_RG_NAME
    reserved            = true
    
    kind = "Linux"
    sku {
        tier = "ElasticPremium"
        size = "EP1"
    }
}
 
resource "azurerm_function_app" "fn-default" {
    
    depends_on = [
        data.azurerm_storage_account.svc-ppl-storage-acc,
        azurerm_application_insights.svc-ppl-appi
     ]

    name = "${var.NAME}-funcn-${var.ENV}"
    location = var.LOCATION
    resource_group_name = var.APP_RG_NAME
    app_service_plan_id = azurerm_app_service_plan.app-plan.id
    storage_account_name        = data.azurerm_storage_account.svc-ppl-storage-acc.name
    storage_account_access_key = data.azurerm_storage_account.svc-ppl-storage-acc.primary_access_key
    version                    = "~3"
    
    identity  {
      type = "SystemAssigned"
    } 

    app_settings = {
        APPINSIGHTS_INSTRUMENTATIONKEY = "${azurerm_application_insights.svc-ppl-appi.instrumentation_key}"
        https_only = true
        
        DOCKER_REGISTRY_SERVER_URL = "https://${var.ACR_URI}"
        DOCKER_REGISTRY_SERVER_USERNAME = "${var.ACR_SP_ID}"
        DOCKER_REGISTRY_SERVER_PASSWORD = "${var.ACR_SP_SECRET}"
        DOCKER_CUSTOM_IMAGE_NAME = "${var.REPO}:latest"
        WEBSITES_ENABLE_APP_SERVICE_STORAGE = false
        FUNCTION_APP_EDIT_MODE = "readonly"

        AUTH_TYPE = "MI"
        KeyVaultEndpoint = "${azurerm_key_vault.kv.vault_uri}"
        KEYVAULT_NAME = "${azurerm_key_vault.kv.name}"
    }

    site_config {
      linux_fx_version  = "DOCKER|${var.ACR_URI}/${var.REPO}:latest"
    }
  
}



# https://registry.terraform.io/providers/hashicorp/azuread/latest/docs/data-sources/service_principal

# If you're authenticating using a Service Principal then it must have permissions 
# to both Read and write all applications and Sign in and read user profile 
# within the Windows Azure Active Directory API.

data "azuread_service_principal" "funcn-system-id" {
   depends_on   = [azurerm_function_app.fn-default]
   display_name = azurerm_function_app.fn-default.name
}


output "function_defaut_name" {
  value = azurerm_function_app.fn-default.name
}

output "APP_FUNCTION_SERVICE_DONE" {
  depends_on  = [ azurerm_function_app.fn-default]
  value       = true
  description = "App Function Service setup is complete"
}
