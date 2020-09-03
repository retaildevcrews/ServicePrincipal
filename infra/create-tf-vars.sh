#!/bin/bash

# check if svc_ppl_Name is valid

Name_Size=${#svc_ppl_Name}
if [[ $Name_Size -lt 5 || $Name_Size -gt 20 ]]
then
  echo "Please set svc_ppl_Name first and make sure it is between 5 and 20 characters in length with no special characters."
  echo $svc_ppl_Name
  echo $Name_Size
  exit 1
fi



# set location to centralus if not set
if [ -z $svc_ppl_Location ]
then
  export svc_ppl_Location=centralus
fi
#  >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>TODO <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
# set repo to serviceprincipal if not set
# if [ -z $svc_ppl_Repo ]
# then
#   export svc_ppl_Repo=serviceprincipal
# fi

# create terraform.tfvars and replace template values






cp example.tfvars terraform.tfvars

# replace name
ex -s -c "%s/<<svc_ppl_Name>>/$svc_ppl_Name/g|x" terraform.tfvars

# replace location
ex -s -c "%s/<<svc_ppl_Location>>/$svc_ppl_Location/g|x" terraform.tfvars

# replace repo
# ex -s -c "%s/<<svc_ppl_Repo>>/$svc_ppl_Repo/g|x" terraform.tfvars

# replace email
# ex -s -c "%s/<<svc_ppl_Email>>/$svc_ppl_Email/g|x" terraform.tfvars

# replace TF_TENANT_ID
ex -s -c "%s/<<svc_ppl_TENANT_ID>>/$(az account show -o tsv --query tenantId)/g|x" terraform.tfvars

# replace TF_SUB_ID
ex -s -c "%s/<<svc_ppl_SUB_ID>>/$(az account show -o tsv --query id)/g|x" terraform.tfvars

# create a service principal
# replace TF_CLIENT_SECRET
ex -s -c "%s/<<svc_ppl_CLIENT_SECRET>>/$(az ad sp create-for-rbac -n http://${svc_ppl_Name}-tf-sp --query password -o tsv)/g|x" terraform.tfvars

# replace TF_CLIENT_ID
ex -s -c "%s/<<svc_ppl_CLIENT_ID>>/$(az ad sp show --id http://${svc_ppl_Name}-tf-sp --query appId -o tsv)/g|x" terraform.tfvars

# create a service principal
# replace ACR_SP_SECRET
# ex -s -c "%s/<<svc_ppl_ACR_SP_SECRET>>/$(az ad sp create-for-rbac --skip-assignment -n http://${svc_ppl_Name}-acr-sp --query password -o tsv)/g|x" terraform.tfvars

# replace ACR_SP_ID
# ex -s -c "%s/<<svc_ppl_ACR_SP_ID>>/$(az ad sp show --id http://${svc_ppl_Name}-acr-sp --query appId -o tsv)/g|x" terraform.tfvars

# validate the substitutions
cat terraform.tfvars