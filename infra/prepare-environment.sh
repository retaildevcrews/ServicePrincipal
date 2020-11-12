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

