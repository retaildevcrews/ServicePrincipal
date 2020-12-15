
#Use this data source to access the configuration of the AzureRM provider.
data "azurerm_client_config" "current" {}

locals {
  kv_name = "kv-${var.NAME}-${var.TENANT_NAME}-${var.ENV}"
  secrets = {
    "SPCosmosKey" = var.COSMOS_RW_KEY,
    "AppInsightsKey" = azurerm_application_insights.instance.instrumentation_key,
    "SPStorageAccountPrimaryKey" = data.azurerm_storage_account.instance.primary_access_key,
    # "graphAppClientId" = var.GRAPH_SP_ID, #azuread_application.graphclient.application_id
    # "graphAppClientSecret" = var.GRAPH_SP_SECRET, #random_password.graphspsecret.result
    # "graphAppTenantId" = var.TENANT_ID,
    "SPStorageConnectionString" = data.azurerm_storage_account.instance.primary_connection_string,
    "SPTfClientId" = var.TF_CLIENT_SP_ID,
    "SPTfClientSecret" = var.TF_CLIENT_SP_SECRET,
    "SPAcrClientId" = var.ACR_SP_ID,
    "SPAcrClientSecret" = var.ACR_SP_SECRET
  }
}

# Create Key Vault // As of today 8-27-2020 v13.1 has some issues https://github.com/hashicorp/terraform/issues/26011
resource azurerm_key_vault instance {
  depends_on = [ data.azurerm_storage_account.instance ]

  name                            = local.kv_name
  location                        = var.LOCATION
  resource_group_name             = var.APP_RG_NAME
  sku_name                        = "standard"
  tenant_id                       = var.TENANT_ID
  enabled_for_deployment          = false
  enabled_for_disk_encryption     = false
  enabled_for_template_deployment = false
}


# https://www.terraform.io/docs/providers/azurerm/r/key_vault_access_policy.html
# NOTE: It's possible to define Key Vault Access Policies both within the azurerm_key_vault resource via the access_policy 
# block and by using the azurerm_key_vault_access_policy resource. 
# However it's not possible to use both methods to manage Access Policies within a KeyVault, since there'll be conflicts.

resource "azurerm_key_vault_access_policy" "terraform-sp" {
  key_vault_id = azurerm_key_vault.instance.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = data.azurerm_client_config.current.object_id

  secret_permissions = [
    "Get",
    "List",
    "Set",
    "Delete"
  ]
}

resource azurerm_key_vault_access_policy instance-pol {
  depends_on = [azurerm_function_app.instance]

  key_vault_id = azurerm_key_vault.instance.id
  tenant_id    = var.TENANT_ID 
  object_id    = azurerm_function_app.instance.identity[0].principal_id 

  secret_permissions = [
    "Get",
    "List",
    "Set"
  ]
}

resource azurerm_key_vault_access_policy fn-staging-slot-policy {
  depends_on = [ azurerm_app_service_slot.staging ]

  key_vault_id = azurerm_key_vault.instance.id
  tenant_id    = var.TENANT_ID 
  object_id    = azurerm_app_service_slot.staging.identity[0].principal_id 

  secret_permissions = [
    "Get",
    "List",
    "Set"
  ]
}

# SECRETS
resource azurerm_key_vault_secret secret {
  for_each = local.secrets

  depends_on = [ azurerm_key_vault_access_policy.terraform-sp ]

  name         = each.key
  value        = each.value
  key_vault_id = azurerm_key_vault.instance.id
}
