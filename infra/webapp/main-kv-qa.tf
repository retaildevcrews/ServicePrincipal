#Use this data source to access the configuration of the AzureRM provider.
locals {
  rgqa_name = "${var.NAME}-rg-qa"
}

resource "azurerm_key_vault" "kvqa" {

  name                            = "${var.NAME}-kv-qa"
  location                        = var.LOCATION
  resource_group_name             = local.rgqa_name
  sku_name                        = "standard"
  tenant_id                       = var.TENANT_ID
  enabled_for_deployment          = false
  enabled_for_disk_encryption     = false
  enabled_for_template_deployment = false

}

resource "azurerm_key_vault_access_policy" "terraform-sp-qa" {
  key_vault_id = azurerm_key_vault.kvqa.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = data.azurerm_client_config.current.object_id

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

resource "azurerm_key_vault_access_policy" "fn-default-pol-qa" {
  depends_on = [azurerm_function_app.fn-default]

  key_vault_id = azurerm_key_vault.kvqa.id
  tenant_id    = var.TENANT_ID # this access policy will get the name of TenantID -- [svc_ppl_Name]
  object_id    = data.azuread_service_principal.funcn-system-id.id

  secret_permissions = [
    "Get",
    "List",
    "Set"
  ]
}

resource "azurerm_key_vault_secret" "cosmosrwkey-qa" {

  depends_on = [
    azurerm_key_vault_access_policy.terraform-sp
  ]

  name         = "SPCosmosKey"
  value        = var.COSMOS_RW_KEY
  key_vault_id = azurerm_key_vault.kv.id
}

resource "azurerm_key_vault_secret" "appinsights-qa" {

  depends_on = [
    azurerm_key_vault_access_policy.terraform-sp
  ]

  name         = "AppInsightsKey"
  value        = azurerm_application_insights.svc-ppl-appi.instrumentation_key
  key_vault_id = azurerm_key_vault.kvqa.id
}

resource "azurerm_key_vault_secret" "storageaccpk-qa" {

  depends_on = [
    azurerm_key_vault_access_policy.terraform-sp
  ]

  name         = "SPStorageAccountPrimaryKey"
  value        = data.azurerm_storage_account.svc-ppl-storage-acc.primary_access_key
  key_vault_id = azurerm_key_vault.kvqa.id
}

resource "azurerm_key_vault_secret" "graphdppclientid-qa" {

  depends_on = [
    azurerm_key_vault_access_policy.terraform-sp
  ]

  name         = "graphAppClientId"
  value        = var.GRAPH_SP_ID #azuread_application.graphclient.application_id
  key_vault_id = azurerm_key_vault.kvqa.id
}

# resource "random_password" "graphspsecret" {
#   length = 35
#   special = true
#   override_special = "~!@#$%&*()-_=+[]{}<>:?"
# }


resource "azurerm_key_vault_secret" "graphdappclientsecret-qa" {

  depends_on = [
    azurerm_key_vault_access_policy.terraform-sp
  ]

  name         = "graphAppClientSecret"
  value        = var.GRAPH_SP_SECRET #random_password.graphspsecret.result
  key_vault_id = azurerm_key_vault.kvqa.id
}

resource "azurerm_key_vault_secret" "graphdapptenantid-qa" {

  depends_on = [
    azurerm_key_vault_access_policy.terraform-sp
  ]

  name         = "graphAppTenantId"
  value        = var.TENANT_ID
  key_vault_id = azurerm_key_vault.kvqa.id
}

resource "azurerm_key_vault_secret" "storageconnectionstring-qa" {

  depends_on = [
    azurerm_key_vault_access_policy.terraform-sp
  ]

  name         = "SPStorageConnectionString"
  value        = data.azurerm_storage_account.svc-ppl-storage-acc.primary_connection_string
  key_vault_id = azurerm_key_vault.kvqa.id
}


resource "azurerm_key_vault_secret" "spterraformclientid-qa" {

  depends_on = [
    azurerm_key_vault_access_policy.terraform-sp
  ]

  name         = "SPTfClientId"
  value        = var.TF_CLIENT_SP_ID
  key_vault_id = azurerm_key_vault.kvqa.id
}

resource "azurerm_key_vault_secret" "spterraformclientsecret-qa" {

  depends_on = [
    azurerm_key_vault_access_policy.terraform-sp
  ]

  name         = "SPTfClientSecret"
  value        = var.TF_CLIENT_SP_SECRET
  key_vault_id = azurerm_key_vault.kvqa.id
}
resource "azurerm_key_vault_secret" "apacrclientid-qa" {

  depends_on = [
    azurerm_key_vault_access_policy.terraform-sp
  ]

  name         = "SPAcrClientId"
  value        = var.ACR_SP_ID
  key_vault_id = azurerm_key_vault.kvqa.id
}
resource "azurerm_key_vault_secret" "apacrclientsecret-qa" {

  depends_on = [
    azurerm_key_vault_access_policy.terraform-sp
  ]

  name         = "SPAcrClientSecret"
  value        = var.ACR_SP_SECRET
  key_vault_id = azurerm_key_vault.kvqa.id
}