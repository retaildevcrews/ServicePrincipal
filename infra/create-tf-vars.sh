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


# store az info into variables
export svc_ppl_TENANT_ID=$(az account show -o tsv --query tenantId)
export svc_ppl_SUB_ID=$(az account show -o tsv --query id)
export svc_ppl_CLIENT_SECRET=$(az ad sp create-for-rbac -n http://${svc_ppl_Name}-tf-sp-${svc_ppl_Enviroment} --query password -o tsv)
export svc_ppl_CLIENT_ID=$(az ad sp show --id http://${svc_ppl_Name}-tf-sp-${svc_ppl_Enviroment} --query appId -o tsv)
export svc_ppl_ACR_SP_SECRET=$(az ad sp create-for-rbac --skip-assignment -n http://${svc_ppl_Name}-acr-sp-${svc_ppl_Enviroment} --query password -o tsv)
export svc_ppl_ACR_SP_ID=$(az ad sp show --id http://${svc_ppl_Name}-acr-sp-${svc_ppl_Enviroment} --query appId -o tsv)

# create terraform.tfvars and replace template values
cat example.tfvars | \
sed "s|<<svc_ppl_Name>>|$svc_ppl_Name|" | \
sed "s|<<svc_ppl_ShortName>>|$svc_ppl_ShortName|" | \
sed "s|<<svc_ppl_Location>>|$svc_ppl_Location|" | \
sed "s|<<svc_ppl_Enviroment>>|$svc_ppl_Enviroment|" | \
sed "s|<<svc_ppl_Repo>>|$svc_ppl_Repo|" | \
# sed "s|<<svc_ppl_Email>>|$svc_ppl_Email|" | \
sed "s|<<svc_ppl_TENANT_ID>>|$svc_ppl_TENANT_ID|" | \
sed "s|<<svc_ppl_SUB_ID>>|$svc_ppl_SUB_ID|" | \
sed "s|<<svc_ppl_CLIENT_SECRET>>|$svc_ppl_CLIENT_SECRET|" | \
sed "s|<<svc_ppl_CLIENT_ID>>|$svc_ppl_CLIENT_ID|" | \
sed "s|<<svc_ppl_ACR_SP_SECRET>>|$svc_ppl_ACR_SP_SECRET|" | \
sed "s|<<svc_ppl_ACR_SP_ID>>|$svc_ppl_ACR_SP_ID|" > terraform.tfvars

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


# create tf_state resource group
export TF_RG_NAME=$svc_ppl_Name-rg-$svc_ppl_Enviroment
echo "Creating the TF Resource Group"
if echo ${TF_RG_NAME} > /dev/null 2>&1 && echo ${svc_ppl_Location} > /dev/null 2>&1; then
    if ! az group create --name ${TF_RG_NAME} --location ${svc_ppl_Location} -o table; then
        echo "ERROR: failed to create the resource group"
        exit 1
    fi
    echo "Created Resource Group: ${TF_RG_NAME} in ${svc_ppl_Location}"
fi

# create storage account for state file
export TFSUB_ID=$(az account show -o tsv --query id)
export TFSA_NAME=$svc_ppl_Name"st"$svc_ppl_Enviroment
echo "Creating File Storage Account and Container"


if echo ${TFSUB_ID} > /dev/null 2>&1; then
    if ! az storage account create --resource-group $TF_RG_NAME --name $TFSA_NAME --sku Standard_LRS --encryption-services blob -o table; then
        echo "ERROR: Failed to create Storage Account"
        exit 1
    fi
    echo "TF Storage Account Created."
    sleep 20s
fi

# retrieve storage account access key
if echo ${TF_RG_NAME} > /dev/null 2>&1; then
    if ! ARM_ACCESS_KEY=$(az storage account keys list --resource-group $TF_RG_NAME --account-name $TFSA_NAME --query [0].value -o tsv); then
        echo "ERROR: Failed to Retrieve Storage Account Access Key"
        exit 1
    fi
    echo "TF Storage Account Access Key = $ARM_ACCESS_KEY"
fi

export TFCI_NAME=$svc_ppl_Name"citfstate"$svc_ppl_Enviroment

if echo ${TF_RG_NAME} > /dev/null 2>&1; then
    if ! az storage container create --name $TFCI_NAME --account-name $TFSA_NAME --account-key $ARM_ACCESS_KEY -o table; then
        echo "ERROR: Failed to Retrieve Storage Container"
        exit 1
    fi
    echo "TF State Storage Account Container Created"
    export TFSA_CONTAINER=$(az storage container show --name ${TFCI_NAME} --account-name ${TFSA_NAME} --account-key ${ARM_ACCESS_KEY} --query name -o tsv)
    echo "TF Storage Container name = ${TFSA_CONTAINER}"
fi

# create storage container 

# echo "The terraform options to store state remotely will be added as main_tf_state.tf in your root directory"
# cat << EOF > main_tf_state.tf
# terraform {
#   required_version = ">= 0.13"
#   backend "azurerm" {
#     resource_group_name  = "${TF_RG_NAME}"
#     storage_account_name = "${TFSA_NAME}"
#     container_name       = "${TFSA_CONTAINER}"
#     key                  = "${svc_ppl_Name}.terraform.tfstate.${svc_ppl_Enviroment}"
#   }
# }
# EOF

