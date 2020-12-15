variable "ENV" {
  type        = string
  description = "The last suffix which should be used for all resources in this example. Used by all modules"
}

variable "STORAGE_ACCOUNT_NAME" {
  type        = string
  description = "Azure Storage Account"
}

variable "QUEUE_NAMES" {
  type = list(string)
  description = "List of the queues to create"
}