locals {
  db_names = toset([ "${var.COSMOS_DB}-${var.ENV}", "${var.COSMOS_DB}-qa" ])

  # create a flattened list that is the cartesian product of db_names and collections
  collections_in_dbs = { for p in setproduct(local.db_names, var.COLLECTIONS) : "${p[0]}.${p[1].name}" => {
      db = p[0]
      collection = p[1]
    }
  }
}
#  --------------CosmosDB instance-----------

resource azurerm_cosmosdb_account instance {
  name                = "cdba-${var.NAME}-${var.ENV}"
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
resource azurerm_cosmosdb_sql_database instance {  
  for_each = local.db_names

  name                = each.key
  resource_group_name = var.APP_RG_NAME
  account_name        = azurerm_cosmosdb_account.instance.name
  throughput          = var.COSMOS_RU

}

# ---->>>> Creating Collections

resource azurerm_cosmosdb_sql_container instance {
  for_each = local.collections_in_dbs

  database_name       = each.value.db
  name                 = each.value.collection.name

  resource_group_name = var.APP_RG_NAME
  account_name        = azurerm_cosmosdb_account.instance.name
  partition_key_path  = each.value.collection.partitionkey
  depends_on = [ azurerm_cosmosdb_sql_database.instance ]
}


output "DB_CREATION_DONE" {
  depends_on  = [azurerm_cosmosdb_account.instance]
  value       = true
  description = "Cosmos Db creation complete"
}

output "RW_KEY" {
  value       = azurerm_cosmosdb_account.instance.primary_master_key
  sensitive   = true
  description = "The read-write key for the instance to be used by the Application. This is used to pass into the webapp module"
}

output COSMOS_ACCOUNT_URI {
  depends_on = [ azurerm_cosmosdb_account.instance ]
  value = azurerm_cosmosdb_account.instance.endpoint
  description = "URI of CosmosDB Account"
}
output "DEV_DATABASE_NAME" {
  value = "${var.COSMOS_DB}-cosmos-${var.ENV}"
}

output "QA_DATABASE_NAME" {
  value = "${var.COSMOS_DB}-cosmos-qa"
}