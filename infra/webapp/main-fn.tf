# NOTE: Storage account name can only consist of lowercase letters and numbers, and must be between 3 and 24 characters long

resource azurerm_storage_account svc-ppl-storage-acc {
  name                      = "${var.NAME}st${var.ENV}"
  location                  = var.LOCATION
  resource_group_name       = var.APP_RG_NAME
  account_tier              = "Standard"
  account_replication_type  = "LRS"
}


output "STORAGE_ACCOUNT_DONE" {
  depends_on  = [azurerm_storage_account.svc-ppl-storage-acc]
  value       = true
  description = "Storage Account setup is complete"
}

output "STORAGE_ACCOUNT_NAME" {
  depends_on  = [azurerm_storage_account.svc-ppl-storage-acc]
  value       = azurerm_storage_account.svc-ppl-storage-acc.name
  description = "Storage Account name"
}


resource "azurerm_app_service_plan" "app-plan" {
    name                = "${var.NAME}-plan-${var.ENV}"
    location            = var.LOCATION
    resource_group_name = var.APP_RG_NAME
    
    kind = "FunctionApp"
    sku {
        tier = "Dynamic"
        size = "Y1"
    }
}
 

resource "azurerm_function_app" "fn-aadfullscan" {
    
    depends_on = [
        azurerm_storage_account.svc-ppl-storage-acc
     ]

    name = "${var.NAME}-funcn-aadfullscan-${var.ENV}"
    location = var.LOCATION
    resource_group_name = var.APP_RG_NAME
    app_service_plan_id = azurerm_app_service_plan.app-plan.id
    storage_account_name        = azurerm_storage_account.svc-ppl-storage-acc.name
    storage_account_access_key = azurerm_storage_account.svc-ppl-storage-acc.primary_access_key
    app_settings = {
        APPINSIGHTS_INSTRUMENTATIONKEY = "${azurerm_application_insights.svc-ppl-appi.instrumentation_key}"

        https_only = true
/*
        FUNCTIONS_WORKER_RUNTIME = "node"
        WEBSITE_NODE_DEFAULT_VERSION = "~10"
        FUNCTION_APP_EDIT_MODE = "readonly"
        
        HASH = "${base64encode(filesha256("${var.functionapp}"))}"

        WEBSITE_RUN_FROM_PACKAGE = "https://${azurerm_storage_account.storage.name}.blob.core.windows.net/${azurerm_storage_container.deployments.name}/${azurerm_storage_blob.appcode.name}${data.azurerm_storage_account_sas.sas.sas}"
    */
    }
  
}

resource "azurerm_function_app" "fn-aaddeltadetector" {

    depends_on = [
        azurerm_storage_account.svc-ppl-storage-acc
     ]

    name = "${var.NAME}-funcn-aaddeltadetector-${var.ENV}"
    location = var.LOCATION
    resource_group_name = var.APP_RG_NAME
    app_service_plan_id = azurerm_app_service_plan.app-plan.id
    storage_account_name        = azurerm_storage_account.svc-ppl-storage-acc.name
    storage_account_access_key = azurerm_storage_account.svc-ppl-storage-acc.primary_access_key
    app_settings = {
        APPINSIGHTS_INSTRUMENTATIONKEY = "${azurerm_application_insights.svc-ppl-appi.instrumentation_key}"

        https_only = true
/*
        FUNCTIONS_WORKER_RUNTIME = "node"
        WEBSITE_NODE_DEFAULT_VERSION = "~10"
        FUNCTION_APP_EDIT_MODE = "readonly"
        
        HASH = "${base64encode(filesha256("${var.functionapp}"))}"

        WEBSITE_RUN_FROM_PACKAGE = "https://${azurerm_storage_account.storage.name}.blob.core.windows.net/${azurerm_storage_container.deployments.name}/${azurerm_storage_blob.appcode.name}${data.azurerm_storage_account_sas.sas.sas}"
    */
    }
}

resource "azurerm_function_app" "fn-aadupdateprocessor" {
    depends_on = [
        azurerm_storage_account.svc-ppl-storage-acc
     ]

    name = "${var.NAME}-funcn-aadupdateprocessor-${var.ENV}"
    location = var.LOCATION
    resource_group_name = var.APP_RG_NAME
    app_service_plan_id = azurerm_app_service_plan.app-plan.id
    storage_account_name        = azurerm_storage_account.svc-ppl-storage-acc.name
    storage_account_access_key = azurerm_storage_account.svc-ppl-storage-acc.primary_access_key
    app_settings = {
        APPINSIGHTS_INSTRUMENTATIONKEY = "${azurerm_application_insights.svc-ppl-appi.instrumentation_key}"

        https_only = true
/*
        FUNCTIONS_WORKER_RUNTIME = "node"
        WEBSITE_NODE_DEFAULT_VERSION = "~10"
        FUNCTION_APP_EDIT_MODE = "readonly"
        
        HASH = "${base64encode(filesha256("${var.functionapp}"))}"

        WEBSITE_RUN_FROM_PACKAGE = "https://${azurerm_storage_account.storage.name}.blob.core.windows.net/${azurerm_storage_container.deployments.name}/${azurerm_storage_blob.appcode.name}${data.azurerm_storage_account_sas.sas.sas}"
    */
    }
}

resource "azurerm_function_app" "fn-graphobjectwriter" {
    depends_on = [
        azurerm_storage_account.svc-ppl-storage-acc
     ]
    
    name = "${var.NAME}-funcn-graphobjectwriter-${var.ENV}"
    location = var.LOCATION
    resource_group_name = var.APP_RG_NAME
    app_service_plan_id = azurerm_app_service_plan.app-plan.id
    storage_account_name        = azurerm_storage_account.svc-ppl-storage-acc.name
    storage_account_access_key   = azurerm_storage_account.svc-ppl-storage-acc.primary_access_key
    app_settings = {
        APPINSIGHTS_INSTRUMENTATIONKEY = "${azurerm_application_insights.svc-ppl-appi.instrumentation_key}"

        https_only = true
/*
        FUNCTIONS_WORKER_RUNTIME = "node"
        WEBSITE_NODE_DEFAULT_VERSION = "~10"
        FUNCTION_APP_EDIT_MODE = "readonly"
        
        HASH = "${base64encode(filesha256("${var.functionapp}"))}"

        WEBSITE_RUN_FROM_PACKAGE = "https://${azurerm_storage_account.storage.name}.blob.core.windows.net/${azurerm_storage_container.deployments.name}/${azurerm_storage_blob.appcode.name}${data.azurerm_storage_account_sas.sas.sas}"
    */
    }
}



output "function_addfullscan_name" {
  value = azurerm_function_app.fn-aadfullscan.name
}

output "function_aaddeltadetector_name" {
  value = azurerm_function_app.fn-aaddeltadetector.name
}

output "function_aadupdateprocessor_name" {
  value = azurerm_function_app.fn-aadupdateprocessor.name
}

output "function_graphobjectwriter_name" {
  value = azurerm_function_app.fn-graphobjectwriter.name
}

output "APP_FUNCTION_SERVICE_DONE" {
  depends_on  = [ azurerm_function_app.fn-aadfullscan, 
                  azurerm_function_app.fn-aaddeltadetector, 
                  azurerm_function_app.fn-aadupdateprocessor,
                  azurerm_function_app.fn-graphobjectwriter
                  ]
  value       = true
  description = "App Function Service setup is complete"
}