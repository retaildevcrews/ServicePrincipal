
variable "ENV" {
  type        = string
  description = "The last suffix which should be used for creating Resource Group only in this example."
}

variable "NAME" {
  type        = string
  description = "The prefix which should be used for all resources in this example. Used by all modules"
}

variable "SHORTNAME" {
  type        = string
  description = "The prefix which should be used for all resources created under Resource Group"
}

variable "LOCATION" {
  type        = string
  description = "The Azure Region in which all resources in this example should be created. Used by all modules"
}


variable "TF_SUB_ID" {
  type        = string
  description = "The Subscription ID for the Terrafrom Service Principal to build resources in. This is only used by the parent main.tf"
}

variable "TF_TENANT_ID" {
  type        = string
  description = "This is the tenant ID of the Azure subscription. This is only used by the parent main.tf"
}

variable "TF_CLIENT_ID" {
  type        = string
  description = "The Client ID(AppID) of the Service Principal that TF will use to Authenticate and build resources as. This account should have at least Contributor Role on the subscription. This is only used by the parent main.tf"

}
variable "TF_CLIENT_SECRET" {
  type        = string
  description = "The Client Secret of the Service Principal that TF will use to Authenticate and build resources as. This account should have at least Contributor Role on the subscription. This is only used by the parent main.tf"
}

variable "COSMOS_RU" {
  type        = number
  description = "The Number of Resource Units allocated to the CosmosDB. This is used by the DB module"
}

variable "ACR_SP_ID" {
  type        = string
  description = "The ACR Service Principal ID"
}

variable "ACR_SP_SECRET" {
  type        = string
  description = "The ACR Service Principal secret"
}

variable "GRAPH_SP_ID" {
  type        = string
  description = "The Graph client Service Principal"
}

variable "GRAPH_SP_SECRET" {
  type        = string
  description = "The Graph client Service Principal Secret"
}


variable "COSMOS_AUDIT_COL" {
  type        = string
  description = "The CosmosDB Collection name used to hold Audit records"
}

variable "COSMOS_CONFIG_COL" {
  type        = string
  description = "The CosmosDB Collection for hold the system's configuration document"
}

variable "COSMOS_OBJ_TRACKING_COL" {
  type        = string
  description = "The Collection used to hold ServicePrincipal and other AAD object items being tracked"
}

variable "COSMOS_ACTIVITY_HISTORY_COL" {
  type        = string
  description = "The Collection used to hold the activity history for long running activities"
}

variable "REPO" {
  type        = string
  description = "The databricks-scim-automation repo"
  default     = "databricks-scim-automation"
}
