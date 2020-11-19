locals {
  db_names = toset([ "${var.COSMOS_DB}-cosmos-${var.ENV}", "${var.COSMOS_DB}-cosmos-qa" ])
}
#  --------------CosmosDB instance-----------

resource "azurerm_cosmosdb_account" "cosmosacct" {
  name                = "${var.NAME}-cosmosa-${var.ENV}"
  location            = var.LOCATION
  resource_group_name = var.APP_RG_NAME
  kind                = "GlobalDocumentDB"
  offer_type          = "Standard"
  consistency_policy {
    consistency_level       = "Session"
    max_interval_in_seconds = "5"
    max_staleness_prefix    = "100"
  }
  enable_automatic_failover = false
  geo_location {
    location          = var.LOCATION
    failover_priority = "0"
  }
}


# ---->>>>  Create a Database
resource "azurerm_cosmosdb_sql_database" "cosmosdb" {  
  for_each = local.db_names

  name                = each.key
  resource_group_name = var.APP_RG_NAME
  account_name        = azurerm_cosmosdb_account.cosmosacct.name
  throughput          = var.COSMOS_RU

}

# ---->>>> Creating Collections

resource "azurerm_cosmosdb_sql_container" "cosmosdb-audit" {
  for_each = local.db_names

  name                = var.COSMOS_AUDIT_COL
  resource_group_name = var.APP_RG_NAME
  account_name        = azurerm_cosmosdb_account.cosmosacct.name
  database_name       = each.key
  partition_key_path  = "/auditYearMonth"
  depends_on = [ azurerm_cosmosdb_sql_database.cosmosdb ]
}

resource "azurerm_cosmosdb_sql_container" "cosmosdb-config" {
  for_each = local.db_names

  name                = var.COSMOS_CONFIG_COL
  resource_group_name = var.APP_RG_NAME
  account_name        = azurerm_cosmosdb_account.cosmosacct.name
  database_name       = each.key
  partition_key_path  = "/configType"
  depends_on = [ azurerm_cosmosdb_sql_database.cosmosdb ]
}

resource "azurerm_cosmosdb_sql_container" "cosmosdb-objtracking" {
  for_each = local.db_names

  name                = var.COSMOS_OBJ_TRACKING_COL
  resource_group_name = var.APP_RG_NAME
  account_name        = azurerm_cosmosdb_account.cosmosacct.name
  database_name       = each.key
  partition_key_path  = "/objectType"
  depends_on = [ azurerm_cosmosdb_sql_database.cosmosdb ]
}

resource "azurerm_cosmosdb_sql_container" "cosmosdb-activityhistory" {
  for_each = local.db_names

  name                = var.COSMOS_ACTIVITY_HISTORY_COL
  resource_group_name = var.APP_RG_NAME
  account_name        = azurerm_cosmosdb_account.cosmosacct.name
  database_name       = each.key
  partition_key_path  = "/correlationId"
  depends_on = [ azurerm_cosmosdb_sql_database.cosmosdb ]
}


output "DB_CREATION_DONE" {
  depends_on  = [azurerm_cosmosdb_account.cosmosacct]
  value       = true
  description = "Cosmos Db creatiom complete"
}

output "RW_KEY" {
  value       = azurerm_cosmosdb_account.cosmosacct.primary_master_key
  sensitive   = true
  description = "The read-write key for the CosmosDB to be used by the Application. This is used to pass into the webapp module"
}

output "DEV_DATABASE_NAME" {
  value = "${var.COSMOS_DB}-cosmos-${var.ENV}"
}

output "QA_DATABASE_NAME" {
  value = "${var.COSMOS_DB}-cosmos-qa"
}