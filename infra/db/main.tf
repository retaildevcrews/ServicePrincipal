
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


output "RO_KEY" {
  value       = azurerm_cosmosdb_account.cosmosacct.primary_readonly_master_key
  sensitive   = true
  description = "The read-only key for the CosmosDB to be used by the Application. This is used to pass into the webapp module"
}

output "RW_KEY" {
  value       = azurerm_cosmosdb_account.cosmosacct.primary_master_key
  sensitive   = true
  description = "The read-write key for the CosmosDB to be used by the Application. This is used to pass into the webapp module"
}

# ---->>>>  Create a Database
resource "azurerm_cosmosdb_sql_database" "cosmosdb" {
=======
output "ro_key" {
  value       = azurerm_cosmosdb_account.cosmosacct.primary_readonly_master_key
  sensitive   = true
  description = "The read Only key for the CosmosDB to be used by the Application. This is used to pass into the webapp module"
}

# ---->>>> Creating Collections
resource "azurerm_cosmosdb_sql_container" "cosmosdb-audit" {
  name                = "Audit"#var.COSMOS_COL
  resource_group_name = var.APP_RG_NAME 
  account_name        = azurerm_cosmosdb_account.cosmosacct.name
  database_name       = azurerm_cosmosdb_sql_database.cosmosdb.name
  partition_key_path  = "/id"
}

resource "azurerm_cosmosdb_sql_container" "cosmosdb-config" {
  name                = "Configuration"#var.COSMOS_COL
  resource_group_name = var.APP_RG_NAME 
  account_name        = azurerm_cosmosdb_account.cosmosacct.name
  database_name       = azurerm_cosmosdb_sql_database.cosmosdb.name
  partition_key_path  = "/id"
}

resource "azurerm_cosmosdb_sql_container" "cosmosdb-objtracking" {
  name                = "ObjectTracking"#var.COSMOS_COL
  resource_group_name = var.APP_RG_NAME 
  account_name        = azurerm_cosmosdb_account.cosmosacct.name
  database_name       = azurerm_cosmosdb_sql_database.cosmosdb.name
  partition_key_path  = "/id"
}

output "DB_CREATION_DONE" {
  depends_on  = [azurerm_cosmosdb_account.cosmosacct]
  value       = true
  description = "Cosmos Db creatiom complete"
}