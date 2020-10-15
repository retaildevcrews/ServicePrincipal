terraform {
  required_providers {
    azurerm = {
        source = "hashicorp/azurerm"
        version = "~>2.24.0"
    }
  }
}

provider "azurerm" {
  version = "~>2.0"
  features {}

  subscription_id = var.TF_SUB_ID
  client_id       = var.TF_CLIENT_ID
  client_secret   = var.TF_CLIENT_SECRET
  tenant_id       = var.TF_TENANT_ID
}

# https://blog.gruntwork.io/terraform-tips-tricks-loops-if-statements-and-gotchas-f739bbae55f9

# Create an application
resource "azuread_application" "example" {
    name = "${var.PREFIX}-${var.BASE_NAME}-${count.index}"
    count = var.NUMBER_TO_GENERATE
}

# Create a service principal
resource "azuread_service_principal" "example" {
  application_id = azuread_application.example[count.index].application_id
  count = var.NUMBER_TO_GENERATE
}
