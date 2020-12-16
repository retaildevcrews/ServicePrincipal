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
  version = "~>2.24"
  features {}

  tenant_id       = var.TF_TENANT_ID
  subscription_id = var.TF_SUB_ID
  # client_id       = var.TF_CLIENT_ID
  # client_secret   = var.TF_CLIENT_SECRET
}

locals {
  rg_name = "rg-${var.NAME}-${var.TENANT_NAME}-${var.ENV}-app"
  storage_acc_name = "${var.NAME}${var.TENANT_NAME}${var.ENV}app"
  queue_names = [ "discover", "evaluate", "update", "discoverqa", "evaluateqa", "updateqa" ]
  collections = [ 
    { 
      name = "Audit"
      partitionkey = "/auditYearMonth"
     }, 
     {
       name="Configuration"
      partitionkey = "/configType"
     }, 
     { 
       name = "ObjectTracking"
      partitionkey = "/objectType"
     }, 
     {
       name = "ActivityHistory"
      partitionkey = "/correlationId"
     }     
  ]
}

resource azurerm_resource_group appResourceGroup {
  name     = local.rg_name
  location = var.LOCATION
}

resource azurerm_storage_account appStorageAccount {
  name                      = local.storage_acc_name
  location                  = azurerm_resource_group.appResourceGroup.location
  resource_group_name       = azurerm_resource_group.appResourceGroup.name

  account_kind              = "StorageV2"
  account_tier              = "Standard"
  access_tier               = "Hot"
  account_replication_type  = "LRS"

  # enable_blob_encryption    = true
  enable_https_traffic_only = true
  is_hns_enabled            = false

  tags = {
    Environment = var.ENV
    Role = "app"
	  Tenant = "lab"
	  Region = var.LOCATION
  }
}

# Create Container Registry
module "acr" {
  depends_on = [ 
    azurerm_resource_group.appResourceGroup 
  ]  
  source        = "./acr"
  NAME          = var.NAME # we are passing the full project name to reduce the chances that the name is already taken 
  LOCATION      = var.LOCATION
  TENANT_NAME   = var.TENANT_NAME
  REPO          = var.REPO
  ENV           = var.ENV
  ACR_RG_NAME   = azurerm_resource_group.appResourceGroup.name
  ACR_SP_ID     = var.ACR_SP_ID
  ACR_SP_SECRET = var.ACR_SP_SECRET
}

# Create Storage Queues
module "asq" {
  depends_on = [ 
    azurerm_storage_account.appStorageAccount 
  ]  
  source        = "./asq"
  ENV           = var.ENV  
  QUEUE_NAMES   = local.queue_names
  STORAGE_ACCOUNT_NAME = azurerm_storage_account.appStorageAccount.name
}


# Create Cosmos Database
module "db" {
  depends_on = [ 
    azurerm_resource_group.appResourceGroup, 
    azurerm_storage_account.appStorageAccount 
  ]  
  source           = "./db"
  NAME             = var.NAME
  LOCATION         = var.LOCATION
  ENV              = var.ENV
  APP_RG_NAME      = azurerm_resource_group.appResourceGroup.name
  COSMOS_RU        = var.COSMOS_RU
  COSMOS_DB        = var.NAME
  COLLECTIONS      = local.collections
}


# Create other Web components that have a direct dependency such as WebApp, Functions, Appinsights, KeyVault etc. 

module "web" {
  depends_on = [ 
    azurerm_resource_group.appResourceGroup, 
    azurerm_storage_account.appStorageAccount 
  ]
  source = "./webapp"
  NAME                = var.SHORTNAME
  PROJECT_NAME        = var.NAME
  LOCATION            = var.LOCATION
  APP_RG_NAME         = azurerm_resource_group.appResourceGroup.name
  STORAGE_NAME        = azurerm_storage_account.appStorageAccount.name
  TENANT_ID           = var.TF_TENANT_ID
  TENANT_NAME         = var.TENANT_NAME
  COSMOS_RW_KEY       = module.db.RW_KEY
  DB_CREATION_DONE    = module.db.DB_CREATION_DONE
  DEV_DATABASE_NAME   = module.db.DEV_DATABASE_NAME
  QA_DATABASE_NAME    = module.db.QA_DATABASE_NAME
  ENV                 = var.ENV
  COSMOS_DB           = var.NAME
  COSMOS_URL          = module.db.COSMOS_ACCOUNT_URI
  REPO                  = var.REPO
  TF_CLIENT_SP_ID       = var.TF_CLIENT_ID
  TF_CLIENT_SP_SECRET   = var.TF_CLIENT_SECRET
  ACR_SP_ID           = var.ACR_SP_ID
  ACR_SP_SECRET       = var.ACR_SP_SECRET
  ACR_URI             = module.acr.acr_uri 
}
