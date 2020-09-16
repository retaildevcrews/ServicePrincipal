variable "NAME" {
  type        = string
  description = "The prefix which should be used for all resources in this example"
}


variable "APP_RG_NAME" {
  type        = string
  description = "The Azure Resource Group the resource should be added to"

}

variable "LOCATION" {
  type        = string
  description = "The Azure Region in which all resources in this example should be created."
}

variable "COSMOS_RU" {
  type    = number
  default = 400
}

variable "COSMOS_DB" {
  type        = string
  description = "The Cosmos DB database name"
  default     = "testDB"
}


variable "ENV" {
  type        = string
  description = "The last suffix which should be used for all resources in this example. Used by all modules"
}

# variable "REPO" {
#   type        = string
#   description = "The Service Principal repo"
# }