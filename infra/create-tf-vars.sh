#!/bin/bash

# check if svc_ppl_Name is valid

Name_Size=${#svc_ppl_Name}
if [[ $Name_Size -lt 3 || $Name_Size -gt 18 ]]
then
  echo "Please set svc_ppl_Name first and make sure it is between 5 and 18 characters in length with no special characters."
  echo $svc_ppl_Name
  echo $Name_Size
  exit 1
fi

# set enviroment to sp if not set
if [ -z $svc_ppl_ShortName ]
then
  export svc_ppl_ShortName=sp
fi


# set location to centralus if not set
if [ -z $svc_ppl_Location ]
then
  export svc_ppl_Location=southcentralus
fi

# set enviroment to dev if not set
if [ -z $svc_ppl_Enviroment ]
then
  export svc_ppl_Enviroment=dev
fi


#  >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>TODO <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
# set repo to serviceprincipal if not set
if [ -z $svc_ppl_Repo ]
then
  export svc_ppl_Repo=serviceprincipal
fi

# create terraform.tfvars and replace template values

cp example.tfvars terraform.tfvars

# replace name
ex -s -c "%s/<<svc_ppl_Name>>/$svc_ppl_Name/g|x" terraform.tfvars

# replace short name
ex -s -c "%s/<<svc_ppl_ShortName>>/$svc_ppl_ShortName/g|x" terraform.tfvars


# replace location
ex -s -c "%s/<<svc_ppl_Location>>/$svc_ppl_Location/g|x" terraform.tfvars

# replace location
ex -s -c "%s/<<svc_ppl_Enviroment>>/$svc_ppl_Enviroment/g|x" terraform.tfvars


# replace repo
ex -s -c "%s/<<svc_ppl_Repo>>/$svc_ppl_Repo/g|x" terraform.tfvars

# replace email
# ex -s -c "%s/<<svc_ppl_Email>>/$svc_ppl_Email/g|x" terraform.tfvars

# replace TF_TENANT_ID
ex -s -c "%s/<<svc_ppl_TENANT_ID>>/$(az account show -o tsv --query tenantId)/g|x" terraform.tfvars

# replace TF_SUB_ID
ex -s -c "%s/<<svc_ppl_SUB_ID>>/$(az account show -o tsv --query id)/g|x" terraform.tfvars

# create a service principal
# replace TF_CLIENT_SECRET
ex -s -c "%s/<<svc_ppl_CLIENT_SECRET>>/$(az ad sp create-for-rbac -n http://${svc_ppl_Name}-tf-sp-${svc_ppl_Enviroment} --query password -o tsv)/g|x" terraform.tfvars

# replace TF_CLIENT_ID
ex -s -c "%s/<<svc_ppl_CLIENT_ID>>/$(az ad sp show --id http://${svc_ppl_Name}-tf-sp-${svc_ppl_Enviroment} --query appId -o tsv)/g|x" terraform.tfvars

# create a service principal
# replace ACR_SP_SECRET
ex -s -c "%s/<<svc_ppl_ACR_SP_SECRET>>/$(az ad sp create-for-rbac --skip-assignment -n http://${svc_ppl_Name}-acr-sp-${svc_ppl_Enviroment} --query password -o tsv)/g|x" terraform.tfvars

# replace ACR_SP_ID
ex -s -c "%s/<<svc_ppl_ACR_SP_ID>>/$(az ad sp show --id http://${svc_ppl_Name}-acr-sp-${svc_ppl_Enviroment} --query appId -o tsv)/g|x" terraform.tfvars

# validate the substitutions
cat terraform.tfvars


# Grant Application.ReadWrite.All and Directory.Read.All API access to Service Principal (${svc_ppl_Name}-tf-sp)
# Get service principal App ID
export servicePricipalId=$(az ad sp list --query "[?appDisplayName=='${svc_ppl_Name}-tf-sp-${svc_ppl_Enviroment}'].appId | [0]" --all) #e5a378f6-a834-4449-9703-c119566dba39
servicePricipalId=$(eval echo $servicePricipalId)
echo "Service Principal AppID: " $servicePricipalId

# Get MSGraphId
export graphId=$(az ad sp list --query "[?appDisplayName=='Microsoft Graph'].appId | [0]" --all) #00000003-0000-0000-c000-000000000000 
graphId=$(eval echo $graphId)
echo "Service MSGraph AppID: " $graphId


# Get MSGraph Permission valiables
export appReadWriteAll=$(az ad sp show --id $graphId --query "oauth2Permissions[?value=='Application.ReadWrite.All'].id | [0]") #06da0dbc-49e2-44d2-8312-53f166ab848a 
appReadWriteAll=$(eval echo $appReadWriteAll)
echo "Application.ReadWrite.All ID: " $appReadWriteAll


export dirReadAll=$(az ad sp show --id $graphId --query "oauth2Permissions[?value=='Directory.Read.All'].id | [0]") #bdfbf15f-ee85-4955-8675-146e8e5296b5
dirReadAll=$(eval echo $dirReadAll)
echo "Directory.Read.All ID:" $dirReadAll

# Add App persmission 
az ad app permission add --id $servicePricipalId --api $graphId --api-permissions $dirReadAll=Scope $appReadWriteAll=Scope

# Make permissions effective
az ad app permission grant --id $servicePricipalId --api $graphId

# Admin consent
az ad app permission admin-consent --id $servicePricipalId





##  Do we need this ?

# # create tf_state resource group
# export TFSTATE_RG_NAME=$He_Name-rg-tf
# echo "Creating the TFState Resource Group"
# if echo ${TFSTATE_RG_NAME} > /dev/null 2>&1 && echo ${He_Location} > /dev/null 2>&1; then
#     if ! az group create --name ${TFSTATE_RG_NAME} --location ${He_Location} -o table; then
#         echo "ERROR: failed to create the resource group"
#         exit 1
#     fi
#     echo "Created Resource Group: ${TFSTATE_RG_NAME} in ${TF_LOCATION}"
# fi

# # create storage account for state file
# export TFSUB_ID=$(az account show -o tsv --query id)
# export TFSA_NAME=tfstate$RANDOM
# echo "Creating State File Storage Account and Container"
# if echo ${TFSUB_ID} > /dev/null 2>&1; then
#     if ! az storage account create --resource-group $TFSTATE_RG_NAME --name $TFSA_NAME --sku Standard_LRS --encryption-services blob -o table; then
#         echo "ERROR: Failed to create Storage Account"
#         exit 1
#     fi
#     echo "TF State Storage Account Created. Name = $TFSA_NAME"
#     sleep 20s
# fi

# # retrieve storage account access key
# if echo ${TFSTATE_RG_NAME} > /dev/null 2>&1; then
#     if ! ARM_ACCESS_KEY=$(az storage account keys list --resource-group $TFSTATE_RG_NAME --account-name $TFSA_NAME --query [0].value -o tsv); then
#         echo "ERROR: Failed to Retrieve Storage Account Access Key"
#         exit 1
#     fi
#     echo "TF State Storage Account Access Key = $ARM_ACCESS_KEY"
# fi

# if echo ${TFSTATE_RG_NAME} > /dev/null 2>&1; then
#     if ! az storage container create --name "container${TFSA_NAME}" --account-name $TFSA_NAME --account-key $ARM_ACCESS_KEY -o table; then
#         echo "ERROR: Failed to Retrieve Storage Container"
#         exit 1
#     fi
#     echo "TF State Storage Account Container Created"
#     export TFSA_CONTAINER=$(az storage container show --name "container${TFSA_NAME}" --account-name ${TFSA_NAME} --account-key ${ARM_ACCESS_KEY} --query name -o tsv)
#     echo "TF Storage Container name = ${TFSA_CONTAINER}"
# fi

# # create storage container 

# echo "The terraform options to store state remotely will be added as main_tf_state.tf in your root directory"
# cat << EOF > ./main_tf_state.tf

# terraform {
#   required_version = ">= 0.13"
#   backend "azurerm" {
#     resource_group_name  = "${TFSTATE_RG_NAME}"
#     storage_account_name = "${TFSA_NAME}"
#     container_name       = "${TFSA_CONTAINER}"
#     key                  = "prod.terraform.tfstate"
#   }
# }

# EOF
