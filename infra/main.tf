provider "azurerm" {
  version = "~>2.0"
  features {}

  subscription_id = var.TF_SUB_ID
  client_id       = var.TF_CLIENT_ID
  client_secret   = var.TF_CLIENT_SECRET
  tenant_id       = var.TF_TENANT_ID
}



# Create Resource Group
resource "azurerm_resource_group" "rg" {
        name = "${var.NAME}-rg-${var.ENV}"
        location = var.LOCATION
}

# Create Container Registry

module "acr" {
  source        = "./acr"
  NAME          = var.NAME # we are passing the full project name to reduce the chances that the name is already taken 
  LOCATION      = var.LOCATION
  REPO          = var.REPO
  ENV           = var.ENV
  ACR_RG_NAME   = azurerm_resource_group.rg.name
  ACR_SP_ID     = var.ACR_SP_ID
  ACR_SP_SECRET = var.ACR_SP_SECRET
}

# Create Storage Queue

module "asq" {
  source        = "./asq"
  NAME          = var.SHORTNAME
  LOCATION      = var.LOCATION
  ENV           = var.ENV  
  APP_RG_NAME   = azurerm_resource_group.rg.name
  STORAGE_ACCOUNT     = module.web.STORAGE_ACCOUNT_NAME
  STORAGE_ACCOUNT_DONE = module.web.STORAGE_ACCOUNT_DONE
}

# Create Cosmos Database
module "db" {
  source           = "./db"
  NAME             = var.SHORTNAME
  LOCATION         = var.LOCATION
  ENV              = var.ENV
  APP_RG_NAME      = azurerm_resource_group.rg.name
  COSMOS_RU        = var.COSMOS_RU
  COSMOS_DB        = var.SHORTNAME
}

# Create other Web components that have a direct deendency such as WebApp, Functions, Appinsights, KeyVault etc. 

module "web" {
  source = "./webapp"
  NAME                = var.SHORTNAME
  PROJECT_NAME        = var.NAME
  LOCATION            = var.LOCATION
  APP_RG_NAME         = azurerm_resource_group.rg.name
  TENANT_ID           = var.TF_TENANT_ID
  COSMOS_RO_KEY       = module.db.RO_KEY
  COSMOS_RW_KEY       = module.db.RW_KEY
  DB_CREATION_DONE    = module.db.DB_CREATION_DONE
  ENV                 = var.ENV
  COSMOS_DB           = var.SHORTNAME
  COSMOS_URL          = "https://${var.SHORTNAME}-cosmosa-${var.ENV}.documents.azure.com:443/"
  REPO                = var.REPO
}
