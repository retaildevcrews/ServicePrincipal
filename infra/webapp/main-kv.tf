
#Use this data source to access the configuration of the AzureRM provider.
data "azurerm_client_config" "current" {}

# Create Key Vault // As of today 8-27-2020 v13.1 has some issues https://github.com/hashicorp/terraform/issues/26011
resource "azurerm_key_vault" "kv" {
  
  depends_on = [azurerm_storage_account.svc-ppl-storage-acc]

  name                            = "${var.NAME}-kv-${var.ENV}"
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
  key_vault_id = azurerm_key_vault.kv.id
  tenant_id = data.azurerm_client_config.current.tenant_id
  object_id = data.azurerm_client_config.current.object_id

  secret_permissions = [
      "Get",
      "List",
      "Set",
      "Delete",
      "Recover",
      "Backup",
      "Restore"
    ]

}

# resource "azurerm_key_vault_access_policy" "web-app-pol" {
#   key_vault_id = azurerm_key_vault.kv.id
#   tenant_id    = var.TENANT_ID  # this access policy will get the name of TenantID -- [svc_ppl_Name]
#   object_id    = azurerm_app_service.web-app.identity.0.principal_id
#   secret_permissions = [
#       "Get",
#       "List"
#   ]

# }

resource "azurerm_key_vault_secret" "cosmosurl" {

   depends_on = [
    azurerm_key_vault_access_policy.terraform-sp
  ]

  name         = "SPCosmosURL"
  value        = var.COSMOS_URL
  key_vault_id = azurerm_key_vault.kv.id
}

resource "azurerm_key_vault_secret" "cosmosrwkey" {

    depends_on = [
    azurerm_key_vault_access_policy.terraform-sp
  ]

  name         = "SPCosmosKey"
  value        = var.COSMOS_RW_KEY
  key_vault_id = azurerm_key_vault.kv.id
}


resource "azurerm_key_vault_secret" "cosmosdatabase" {

  depends_on = [
    azurerm_key_vault_access_policy.terraform-sp
  ]

  name         = "SPCosmosDatabase"
  value        = "${var.COSMOS_DB}-cosmos-${var.ENV}"
  key_vault_id = azurerm_key_vault.kv.id
}


resource "azurerm_key_vault_secret" "cosmosauditcol" {
  
  depends_on = [
    azurerm_key_vault_access_policy.terraform-sp
  ]
  
  name         = "SPAuditCollection"
  value        = var.COSMOS_AUDIT_COL
  key_vault_id = azurerm_key_vault.kv.id
}


resource "azurerm_key_vault_secret" "cosmosconfigcol" {
  
  depends_on = [
    azurerm_key_vault_access_policy.terraform-sp
  ]
  
  name         = "SPConfigurationCollection"
  value        = var.COSMOS_CONFIG_COL
  key_vault_id = azurerm_key_vault.kv.id
}

resource "azurerm_key_vault_secret" "cosmosobktrackingcol" {
  
  depends_on = [
    azurerm_key_vault_access_policy.terraform-sp
  ]
  
  name         = "SPObjectTracking"
  value        = var.COSMOS_OBJ_TRACKING_COL
  key_vault_id = azurerm_key_vault.kv.id
}


resource "azurerm_key_vault_secret" "appinsights" {
  
  depends_on = [
    azurerm_key_vault_access_policy.terraform-sp
  ]

  name         = "AppInsightsKey"
  value        = azurerm_application_insights.svc-ppl-appi.instrumentation_key
  key_vault_id = azurerm_key_vault.kv.id
}

resource "azurerm_key_vault_secret" "storageaccpk" {
  
  depends_on = [
    azurerm_key_vault_access_policy.terraform-sp
  ]

  name         = "SPStorageAccountPrimaryKey"
  value        = azurerm_storage_account.svc-ppl-storage-acc.primary_access_key
  key_vault_id = azurerm_key_vault.kv.id
}

resource "azurerm_key_vault_secret" "aadupdatequeue" {
  
  depends_on = [
    azurerm_key_vault_access_policy.terraform-sp
  ]

  name         = "SPAADUpdateQueue"
  value        = var.AADUPDATE_QUEUE_NAME
  key_vault_id = azurerm_key_vault.kv.id
}

resource "azurerm_key_vault_secret" "trackingupdatequeue" {
  
  depends_on = [
    azurerm_key_vault_access_policy.terraform-sp
  ]

  name         = "SPTrackingUpdateQueue"
  value        = var.TRACKING_QUEUE_NAME
  key_vault_id = azurerm_key_vault.kv.id
}