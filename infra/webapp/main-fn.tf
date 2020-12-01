# NOTE: Storage account name can only consist of lowercase letters and numbers, and must be between 3 and 24 characters long

data "azurerm_storage_account" "svc-ppl-storage-acc" {
  name                = var.STORAGE_NAME
  resource_group_name = var.APP_RG_NAME
}

# If you're authenticating using a Service Principal then it must have permissions 
# to both Read and write all applications and Sign in and read user profile 
# within the Windows Azure Active Directory API.



output "STORAGE_ACCOUNT_DONE" {
  depends_on = [data.azurerm_storage_account.svc-ppl-storage-acc
  ]
  value       = true
  description = "Storage Account setup is complete"
}

resource "azurerm_app_service_plan" "app-plan" {
  name                = "${var.NAME}-plan-${var.ENV}"
  location            = var.LOCATION
  resource_group_name = var.APP_RG_NAME
  reserved            = true

  kind = "elastic"
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

  name                       = "${var.NAME}-funcn-${var.ENV}"
  location                   = var.LOCATION
  resource_group_name        = var.APP_RG_NAME
  app_service_plan_id        = azurerm_app_service_plan.app-plan.id
  storage_account_name       = data.azurerm_storage_account.svc-ppl-storage-acc.name
  storage_account_access_key = data.azurerm_storage_account.svc-ppl-storage-acc.primary_access_key
  version                    = "~3"
  os_type                    = "linux"
  https_only                 = true

  identity {
    type = "SystemAssigned"
  }

  site_config {
    linux_fx_version = "DOCKER|${var.ACR_URI}/${var.REPO}:latest"
    use_32_bit_worker_process = false
  }

  # logs {
  #     application_logs {
  #         file_system_level = "Off"
  #     }

  #     http_logs {

  #         file_system {
  #             retention_in_days = 5
  #             retention_in_mb   = 35
  #         }
  #     }
  # }

  app_settings = {
    APPINSIGHTS_INSTRUMENTATIONKEY = azurerm_application_insights.svc-ppl-appi.instrumentation_key
    FUNCTIONS_WORKER_RUNTIME       = "dotnet"
    FUNCTIONS_EXTENSION_VERSION    = "~3"
    FUNCTION_APP_EDIT_MODE              = "readonly"

    WEBSITE_CONTENTAZUREFILECONNECTIONSTRING =  data.azurerm_storage_account.svc-ppl-storage-acc.primary_connection_string
    WEBSITE_CONTENTSHARE                =  "sp-funcn-dev-content"
    WEBSITES_ENABLE_APP_SERVICE_STORAGE = false



    DOCKER_REGISTRY_SERVER_URL          = "https://${var.ACR_URI}"
    DOCKER_REGISTRY_SERVER_USERNAME     = var.ACR_SP_ID
    DOCKER_REGISTRY_SERVER_PASSWORD     = var.ACR_SP_SECRET
    DOCKER_CUSTOM_IMAGE_NAME            = "${var.REPO}:latest"
    AUTH_TYPE     = "MI"
    KEYVAULT_NAME = azurerm_key_vault.kv.name

    # SLOT SPECIFIC SETTINGS - THESE SHOULD BE OVERWRITTED WITH CD PIPELINE
    SPStorageConnectionString = data.azurerm_storage_account.svc-ppl-storage-acc.primary_connection_string
    SPCosmosURL = var.COSMOS_URL
    SPCosmosDatabase = var.DEV_DATABASE_NAME
    SPDiscoverQueue = "discover"
    SPEvaluateQueue = "evaluate"
    SPUpdateQueue = "update"   
    SPConfigurationCollection = "Configuration"
    SPObjectTrackingCollection = "ObjectTracking"
    SPAuditCollection = "Audit"
    SPActivityHistoryCollection = "ActivityHistory"

  }


}


# data "azuread_service_principal" "funcn-system-id" {
#   depends_on   = [azurerm_function_app.fn-default]
#   display_name = azurerm_function_app.fn-default.name
# }


resource "azurerm_app_service_slot" "service-slot-staging" {
  name                = "staging"
  app_service_name    = azurerm_function_app.fn-default.name
  location            = var.LOCATION
  resource_group_name = var.APP_RG_NAME
  app_service_plan_id = azurerm_app_service_plan.app-plan.id
  https_only                 = true

  identity {
    type = "SystemAssigned"
  }

  site_config {
    use_32_bit_worker_process = false
  }

  logs {
      application_logs {
          file_system_level = "Off"
      }

      http_logs {

          file_system {
              retention_in_days = 5
              retention_in_mb   = 35
          }
      }
  }

  app_settings = {
    APPINSIGHTS_INSTRUMENTATIONKEY = azurerm_application_insights.svc-ppl-appi.instrumentation_key
    FUNCTIONS_WORKER_RUNTIME       = "dotnet"
    FUNCTIONS_EXTENSION_VERSION    = "~3"
    FUNCTION_APP_EDIT_MODE              = "readonly"

    WEBSITE_CONTENTAZUREFILECONNECTIONSTRING =  data.azurerm_storage_account.svc-ppl-storage-acc.primary_connection_string
    WEBSITE_CONTENTSHARE                =  "sp-funcn-dev-content"
    WEBSITES_ENABLE_APP_SERVICE_STORAGE = false

    https_only                     = true

    DOCKER_REGISTRY_SERVER_URL          = "https://${var.ACR_URI}"
    DOCKER_REGISTRY_SERVER_USERNAME     = var.ACR_SP_ID
    DOCKER_REGISTRY_SERVER_PASSWORD     = var.ACR_SP_SECRET
    DOCKER_CUSTOM_IMAGE_NAME            = ""
    AUTH_TYPE     = "MI"
    KEYVAULT_NAME = azurerm_key_vault.kv.name


    # SLOT SPECIFIC SETTINGS - THESE SHOULD BE OVERWRITTED WITH CD PIPELINE
    SPStorageConnectionString = data.azurerm_storage_account.svc-ppl-storage-acc.primary_connection_string
    SPCosmosURL = var.COSMOS_URL
    SPCosmosDatabase = var.QA_DATABASE_NAME
    SPDiscoverQueue = "discoverqa"
    SPEvaluateQueue = "evaluateqa"
    SPUpdateQueue = "updateqa"
    SPConfigurationCollection = "Configuration"
    SPObjectTrackingCollection = "ObjectTracking"
    SPAuditCollection = "Audit"
    SPActivityHistoryCollection = "ActivityHistory"
    aadUpdateMode = "ReportOnly"
    "AzureWebJobs.Discover.Disabled" = "False"
    "AzureWebJobs.DiscoverDeltas.Disabled" = "True"
    "AzureWebJobs.Evaluate.Disabled" = "False"
    "AzureWebJobs.UpdateAAD.Disabled" = "True"
  }
}
# https://registry.terraform.io/providers/hashicorp/azuread/latest/docs/data-sources/service_principal



output "function_defaut_name" {
  value = azurerm_function_app.fn-default.name
}

output "APP_FUNCTION_SERVICE_DONE" {
  depends_on  = [azurerm_function_app.fn-default]
  value       = true
  description = "App Function Service setup is complete"
}
