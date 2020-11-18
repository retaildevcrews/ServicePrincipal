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

variable "ENV" {
  type        = string
  description = "The last suffix which should be used for all resources in this example. Used by all modules"
}


variable "STORAGE_ACCOUNT_NAME" {
  type        = string
  description = "Azure Storage Account"
}

variable "STORAGE_ACCOUNT_DONE" {
  description = "Storage Account dependency complete"
  type        = bool
}

variable "QUEUE_NAMES" {
  type = list(string)
  description = "List of the queues to create"
}