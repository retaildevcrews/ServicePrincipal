terraform {
  required_version = ">= 0.13"
  backend "azurerm" {
     #These values must be set during terraform init  
    resource_group_name  = ""
    storage_account_name = ""
    container_name       = ""
    key                  = ""
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

# Create Resource Group - this is created when executing "create-tf-vars.sh" file
# resource "azurerm_resource_group" "rg" {
#         name = "${var.NAME}-rg-${var.ENV}"
#         location = var.LOCATION
# }

locals {
  rg_name = "${var.NAME}-rg-${var.ENV}"
  storage_acc_name = "${var.NAME}st${var.ENV}"
}

# Create Container Registry

module "acr" {
  source        = "./acr"
  NAME          = var.NAME # we are passing the full project name to reduce the chances that the name is already taken 
  LOCATION      = var.LOCATION
  REPO          = var.REPO
  ENV           = var.ENV
  ACR_RG_NAME   = local.rg_name#azurerm_resource_group.rg.name
  ACR_SP_ID     = var.ACR_SP_ID
  ACR_SP_SECRET = var.ACR_SP_SECRET
}

# Create Storage Queue

module "asq" {
  source        = "./asq"
  NAME          = var.SHORTNAME
  LOCATION      = var.LOCATION
  ENV           = var.ENV  
  APP_RG_NAME   = local.rg_name#azurerm_resource_group.rg.name
  STORAGE_ACCOUNT_NAME     = local.storage_acc_name
  STORAGE_ACCOUNT_DONE = module.web.STORAGE_ACCOUNT_DONE
}

# Create Cosmos Database
module "db" {
  source           = "./db"
  NAME             = var.SHORTNAME
  LOCATION         = var.LOCATION
  ENV              = var.ENV
  APP_RG_NAME      = local.rg_name#azurerm_resource_group.rg.name
  COSMOS_RU        = var.COSMOS_RU
  COSMOS_DB        = var.SHORTNAME
  COSMOS_AUDIT_COL = var.COSMOS_AUDIT_COL
  COSMOS_CONFIG_COL = var.COSMOS_CONFIG_COL
  COSMOS_OBJ_TRACKING_COL = var.COSMOS_OBJ_TRACKING_COL
}

# Create other Web components that have a direct deendency such as WebApp, Functions, Appinsights, KeyVault etc. 

module "web" {
  source = "./webapp"
  NAME                = var.SHORTNAME
  PROJECT_NAME        = var.NAME
  LOCATION            = var.LOCATION
  APP_RG_NAME         = local.rg_name#azurerm_resource_group.rg.name
  STORAGE_NAME        = local.storage_acc_name
  TENANT_ID           = var.TF_TENANT_ID
  COSMOS_RW_KEY       = module.db.RW_KEY
  DB_CREATION_DONE    = module.db.DB_CREATION_DONE
  ENV                 = var.ENV
  COSMOS_DB           = var.SHORTNAME
  COSMOS_URL          = "https://${var.SHORTNAME}-cosmosa-${var.ENV}.documents.azure.com:443/"
  COSMOS_AUDIT_COL    = var.COSMOS_AUDIT_COL
  COSMOS_CONFIG_COL   = var.COSMOS_CONFIG_COL
  COSMOS_OBJ_TRACKING_COL = var.COSMOS_OBJ_TRACKING_COL
  EVALUATE_QUEUE_NAME     = module.asq.EVALUATE_QUEUE_NAME
  UPDATE_QUEUE_NAME      = module.asq.UPDATE_QUEUE_NAME
  REPO                  = var.REPO
  TF_CLIENT_SP_ID       = var.TF_CLIENT_ID
  TF_CLIENT_SP_SECRET   = var.TF_CLIENT_SECRET
  ACR_SP_ID           = var.ACR_SP_ID
  ACR_SP_SECRET       = var.ACR_SP_SECRET
  GRAPH_SP_ID           = var.GRAPH_SP_ID
  GRAPH_SP_SECRET       = var.GRAPH_SP_SECRET
  ACR_URI             = module.acr.acr_uri 
}
