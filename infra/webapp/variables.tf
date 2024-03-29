variable "NAME" {
  type        = string
  description = "The prefix which should be used for all resources in this example"

}

variable "PROJECT_NAME" {
  type        = string
  description = "The prefix which should be used for Storage Account resource in this example"

}

variable "TENANT_NAME" {
  type        = string
  description = "The short name of the tenant. Used for resource naming"
}

variable "APP_RG_NAME" {
  type        = string
  description = "The Azure Resource Group the resource should be added to"

}

variable "LOCATION" {
  type        = string
  description = "The Azure Region in which all resources in this example should be created."

}


variable "TENANT_ID" {
  type        = string
  description = "This is the tenant ID of the Azure subscription."
}

variable "COSMOS_URL" {
  type        = string
  description = "This is the primary connection string of the Cosmos DB and will be an output from the resource command."

}

variable "COSMOS_RW_KEY" {
  description = "TThis is the primary read-write key to connect to the Cosmos DB"

}

variable "COSMOS_DB" {
  type        = string
  description = "This is the database name of the Cosmos DB and will be an output from the resource command."

}

variable "DEV_DATABASE_NAME" {
  type = string
  description = "Name of the dev cosmos database"
}

variable "QA_DATABASE_NAME" {
  type = string
  description = "Name of the qa cosmos database"
}

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

variable "TF_CLIENT_SP_ID" {
  type        = string
  description = "The Terraform Service Principal"
}

variable "TF_CLIENT_SP_SECRET" {
  type        = string
  description = "The Terraform Service Principal Secrete"
}
variable "ACR_SP_ID" {
  type        = string
  description = "The ACR Service Principal"
}

variable "ACR_SP_SECRET" {
  type        = string
  description = "The ACR Service Principal Secret"
}

variable "STORAGE_NAME" {
  type        = string
  description = "The Azure Storage Account Name"
}

variable "ACR_URI" {
  type        = string
  description = "Container Registry URI"
}
