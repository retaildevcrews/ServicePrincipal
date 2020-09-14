variable "NAME" {
  type        = string
  description = "The prefix which should be used for all resources in this example"

}

variable "PROJECT_NAME" {
  type        = string
  description = "The prefix which should be used for Storage Account resource in this example"

}

variable "APP_RG_NAME" {
  type        = string
  description = "The Azure Resource Group the resource should be added to"

}

variable "LOCATION" {
  type        = string
  description = "The Azure Region in which all resources in this example should be created."

}

# variable "APP_SERVICE_DONE" {
#   description = "App Service dependency complete"
#   type        = bool
# }

variable "TENANT_ID" {
  type        = string
  description = "This is the tenant ID of the Azure subscription."
}

variable "COSMOS_URL" {
  type        = string
  description = "This is the primary connection string of the Cosmos DB and will be an output from the resource command."

}
variable "COSMOS_RO_KEY" {
  description = "This is the managed identify read-only key from the Cosmos DB and will be an output from the resource command."

}

variable "COSMOS_RW_KEY" {
  description = "This is the managed identify read-write key from the Cosmos DB and will be an output from the resource command."

}


variable "COSMOS_DB" {
  type        = string
  description = "This is the database name of the Cosmos DB and will be an output from the resource command."

}

/*
variable "COSMOS_COL" {
  type        = string
  description = "This is the collection name of the Cosmos DB and will be an output from the resource command."
}
*/

variable "DB_CREATION_DONE" {
  description = "Cosmos DB creation done"
  type        = bool
}

variable "REPO" {
  type        = string
  description = "The Service Principal repo"
}

variable "ENV" {
  type        = string
  description = "The last suffix which should be used for all resources in this example. Used by all modules"
}


