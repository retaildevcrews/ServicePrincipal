#!/bin/bash

ACCOUNT=$(az account show)
if [ $? -eq 1 ]
then
  echo "Please login to Azure first"
  exit 1
fi


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
  export svc_ppl_Location=centralus
fi

# set enviroment to dev if not set
if [ -z $svc_ppl_Environment ]
then
  export svc_ppl_Environment=dev
fi


# set repo to serviceprincipal if not set
if [ -z $svc_ppl_Repo ]
then
  export svc_ppl_Repo=serviceprincipal
fi

KEYVAULT_NAME="${svc_ppl_ShortName}-kv-${svc_ppl_Environment}"
# store az info into variables
export svc_ppl_TENANT_ID=$(echo $ACCOUNT | jq -r ".tenantId")
export svc_ppl_SUB_ID=$(echo $ACCOUNT | jq -r ".id")

export svc_ppl_CLIENT_SECRET=$(az keyvault secret show --vault-name $KEYVAULT_NAME --name SPTfClientSecret | jq -r ".value") #$(az ad sp create-for-rbac -n http://${svc_ppl_Name}-tf-sp-${svc_ppl_Environment} --query password -o tsv)
export svc_ppl_CLIENT_ID=$(az keyvault secret show --vault-name $KEYVAULT_NAME --name SPTfClientId | jq -r ".value") #$(az ad sp show --id http://${svc_ppl_Name}-tf-sp-${svc_ppl_Environment} --query appId -o tsv)
export svc_ppl_ACR_SP_SECRET=$(az keyvault secret show --vault-name $KEYVAULT_NAME --name SPAcrClientSecret | jq -r ".value") #$(az ad sp create-for-rbac --skip-assignment -n http://${svc_ppl_Name}-acr-sp-${svc_ppl_Environment} --query password -o tsv)
export svc_ppl_ACR_SP_ID=$(az keyvault secret show --vault-name $KEYVAULT_NAME --name SPAcrClientId | jq -r ".value") #$(az ad sp show --id http://${svc_ppl_Name}-acr-sp-${svc_ppl_Environment} --query appId -o tsv)
export svc_ppl_GRAPH_SP_SECRET=$(az keyvault secret show --vault-name $KEYVAULT_NAME --name graphAppClientSecret | jq -r ".value") #$(az ad sp create-for-rbac --skip-assignment -n http://${svc_ppl_Name}-graph-${svc_ppl_Environment} --query password -o tsv)
export svc_ppl_GRAPH_SP_ID=$(az keyvault secret show --vault-name $KEYVAULT_NAME --name graphAppClientId | jq -r ".value") #$(az ad sp show --id http://${svc_ppl_Name}-graph-${svc_ppl_Environment} --query appId -o tsv)



# create terraform.tfvars and replace template values
cat example.tfvars | \
sed "s|<<svc_ppl_Name>>|$svc_ppl_Name|" | \
sed "s|<<svc_ppl_ShortName>>|$svc_ppl_ShortName|" | \
sed "s|<<svc_ppl_Location>>|$svc_ppl_Location|" | \
sed "s|<<svc_ppl_Environment>>|$svc_ppl_Environment|" | \
sed "s|<<svc_ppl_Repo>>|$svc_ppl_Repo|" | \
# sed "s|<<svc_ppl_Email>>|$svc_ppl_Email|" | \
sed "s|<<svc_ppl_TENANT_ID>>|$svc_ppl_TENANT_ID|" | \
sed "s|<<svc_ppl_SUB_ID>>|$svc_ppl_SUB_ID|" | \
sed "s|<<svc_ppl_CLIENT_SECRET>>|$svc_ppl_CLIENT_SECRET|" | \
sed "s|<<svc_ppl_CLIENT_ID>>|$svc_ppl_CLIENT_ID|" | \
sed "s|<<svc_ppl_ACR_SP_SECRET>>|$svc_ppl_ACR_SP_SECRET|" | \
sed "s|<<svc_ppl_ACR_SP_ID>>|$svc_ppl_ACR_SP_ID|" | \
sed "s|<<svc_ppl_GRAPH_SP_ID>>|$svc_ppl_GRAPH_SP_ID|" | \
sed "s|<<svc_ppl_GRAPH_SP_SECRET>>|$svc_ppl_GRAPH_SP_SECRET|" > terraform.tfvars

# validate the substitutions
cat terraform.tfvars


# Grant Application.ReadWrite.All and Directory.Read.All API access to Service Principal (${svc_ppl_Name}-tf-sp)
# Get service principal App ID
# export servicePricipalId=$svc_ppl_CLIENT_ID #$(az ad sp list --query "[?appDisplayName=='${svc_ppl_Name}-tf-sp-${svc_ppl_Environment}'].appId | [0]" --all) 
# servicePricipalId=$(eval echo $servicePricipalId)
# echo "Service Principal AppID: " $servicePricipalId

# # Get MSGraphId
# export graphId=$(az ad sp list --query "[?appDisplayName=='Microsoft Graph'].appId | [0]" --all) 
# graphId=$(eval echo $graphId)
# echo "Service MSGraph AppID: " $graphId


# # Get MSGraph Permission variables
# export appReadWriteAll=$(az ad sp show --id $graphId --query "oauth2Permissions[?value=='Application.ReadWrite.All'].id | [0]") 
# appReadWriteAll=$(eval echo $appReadWriteAll)
# echo "Application.ReadWrite.All ID: " $appReadWriteAll


# export dirReadAll=$(az ad sp show --id $graphId --query "oauth2Permissions[?value=='Directory.Read.All'].id | [0]") 
# dirReadAll=$(eval echo $dirReadAll)
# echo "Directory.Read.All ID:" $dirReadAll

# # Add App persmission 
# az ad app permission add --id $servicePricipalId --api $graphId --api-permissions $dirReadAll=Scope $appReadWriteAll=Scope

# # Make permissions effective
# az ad app permission grant --id $servicePricipalId --api $graphId

# # Admin consent
# az ad app permission admin-consent --id $servicePricipalId

# Get graph service principal App ID--------------------------------------------------------------------------------------------

# export graphServicePricipalId=$svc_ppl_GRAPH_SP_ID
# servicePricipalId=$(eval echo $graphServicePricipalId)
# echo "Graph Service Principal AppID: " $graphServicePricipalId

# export appRoleAppReadWriteAll=$(az ad sp show --id $graphId --query "appRoles[?value=='Application.ReadWrite.All'].id | [0]") 
# appRoleAppReadWriteAll=$(eval echo $appRoleAppReadWriteAll)
# echo "Application- Application.ReadWrite.All ID: " $appRoleAppReadWriteAll


# export appRoleDirReadAll=$(az ad sp show --id $graphId --query "appRoles[?value=='Directory.Read.All'].id | [0]") 
# appRoleDirReadAll=$(eval echo $appRoleDirReadAll)
# echo "Application- Directory.Read.All ID:" $appRoleDirReadAll


# # Add App persmission 
# az ad app permission add --id $graphServicePricipalId --api $graphId --api-permissions $appRoleDirReadAll=Role $appRoleAppReadWriteAll=Role

# # Make permissions effective
# az ad app permission grant --id $graphServicePricipalId --api $graphId

# # Admin consent
# az ad app permission admin-consent --id $graphServicePricipalId




# create tf_state resource group
export TF_RG_NAME=$svc_ppl_Name-rg-$svc_ppl_Environment
# echo "Creating the TF Resource Group"
# if echo ${TF_RG_NAME} > /dev/null 2>&1 && echo ${svc_ppl_Location} > /dev/null 2>&1; then
#     if ! az group create --name ${TF_RG_NAME} --location ${svc_ppl_Location} -o table; then
#         echo "ERROR: failed to create the resource group"
#         exit 1
#     fi
#     echo "Created Resource Group: ${TF_RG_NAME} in ${svc_ppl_Location}"
# fi

# create storage account for state file
export TFSUB_ID=$svc_ppl_TENANT_ID
export TFSA_NAME=$svc_ppl_Name"st"$svc_ppl_Environment
# echo "Creating File Storage Account and Container"


# if echo ${TFSUB_ID} > /dev/null 2>&1; then
#     if ! az storage account create --resource-group $TF_RG_NAME --name $TFSA_NAME --sku Standard_LRS --encryption-services blob -o table; then
#         echo "ERROR: Failed to create Storage Account"
#         exit 1
#     fi
#     echo "TF Storage Account Created."
#     sleep 20s
# fi

# # retrieve storage account access key
# if echo ${TF_RG_NAME} > /dev/null 2>&1; then
#     if ! ARM_ACCESS_KEY=$(az storage account keys list --resource-group $TF_RG_NAME --account-name $TFSA_NAME --query [0].value -o tsv); then
#         echo "ERROR: Failed to Retrieve Storage Account Access Key"
#         exit 1
#     fi
#     echo "TF Storage Account Access Key = $ARM_ACCESS_KEY"
# fi

export TFCI_NAME=$svc_ppl_Name"citfstate"$svc_ppl_Environment
export TFSA_CONTAINER=$TFCI_NAME
# if echo ${TF_RG_NAME} > /dev/null 2>&1; then
#     if ! az storage container create --name $TFCI_NAME --account-name $TFSA_NAME --account-key $ARM_ACCESS_KEY -o table; then
#         echo "ERROR: Failed to Retrieve Storage Container"
#         exit 1
#     fi
#     echo "TF State Storage Account Container Created"
#     export TFSA_CONTAINER=$(az storage container show --name ${TFCI_NAME} --account-name ${TFSA_NAME} --account-key ${ARM_ACCESS_KEY} --query name -o tsv)
#     echo "TF Storage Container name = ${TFSA_CONTAINER}"
# fi

# create storage container 

# echo "The terraform options to store state remotely will be added as main_tf_state.tf in your root directory"
# cat << EOF > main_tf_state.tf
# terraform {
#   required_version = ">= 0.13"
#   backend "azurerm" {
#     resource_group_name  = "${TF_RG_NAME}"
#     storage_account_name = "${TFSA_NAME}"
#     container_name       = "${TFSA_CONTAINER}"
#     key                  = "${svc_ppl_Name}.terraform.tfstate.${svc_ppl_Environment}"
#   }
# }
# EOF

