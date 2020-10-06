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

# create terraform.tfvars and replace template values

cp example.tfvars terraform.tfvars

# replace TF_TENANT_ID
ex -s -c "%s/<<svc_ppl_TENANT_ID>>/$(az account show -o tsv --query tenantId)/g|x" terraform.tfvars

# replace TF_SUB_ID
ex -s -c "%s/<<svc_ppl_SUB_ID>>/$(az account show -o tsv --query id)/g|x" terraform.tfvars

# create a service principal
# replace TF_CLIENT_SECRET
ex -s -c "%s/<<svc_ppl_CLIENT_SECRET>>/$(az ad sp create-for-rbac -n http://${svc_ppl_Name}-tf-sp-${svc_ppl_Enviroment} --query password -o tsv)/g|x" terraform.tfvars

# replace TF_CLIENT_ID
ex -s -c "%s/<<svc_ppl_CLIENT_ID>>/$(az ad sp show --id http://${svc_ppl_Name}-tf-sp-${svc_ppl_Enviroment} --query appId -o tsv)/g|x" terraform.tfvars

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


