#!/bin/bash

ACCOUNT=$(az account show)
if [ $? -eq 1 ]
then
  echo "Please login to Azure first"
  exit 1
fi

function parse_args()
{
  red=`tput setaf 1`
  green=`tput setaf 2`
  yellow=`tput setaf 3`
  reset=`tput sgr0`
  die() { echo "$*" >&2; exit 2; }  # complain to STDERR and exit with error
  needs_arg() { if [ -z "$OPTARG" ]; then die "No arg for --$OPT option"; fi; }

  # PARSE ARGUMENTS
  # -? - whatif
  # -f - first run

  WHAT_IF=0
  FIRST_RUN=0
  NO_CLOBBER=0
  while getopts "hfwn" opt; do
    case ${opt} in
      h ) # process option h
        echo "Usage: prepare-terraform [-h] [-f] [-w]"
        echo "       -h  this help message"
        echo "       -f  first-run"
        echo "       -w  what-if"
        exit 1
        ;;
      f ) # process option f
        FIRST_RUN=1
        ;;
      n ) # process option n
        NO_CLOBBER=1
        ;;        
      w ) # process option w
        WHAT_IF=1
        ;;
      ? ) 
        echo "Usage: prepare-terraform [-h] [-f] [-w] [-n]"
        exit 1
        ;;
    esac
  done

  # echo "FIRST_RUN: $FIRST_RUN"
  # echo "WHAT_IF: $WHAT_IF"
  # echo "NO_CLOBBER: $NO_CLOBBER"
  # echo ""
}

function validate_environment()
{
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


  if [ -z $svc_ppl_TenantName ]
  then
    export svc_ppl_TenantName=cse
  fi

  # Build the resource names to see if we will have a name length issue
  tmp_name="${svc_ppl_Name}${svc_ppl_TenantName}${svc_ppl_Environment}tf"
  export TFSA_NAME=${tmp_name,,}  

  Name_Size=${#TFSA_NAME}
  if [[ $Name_Size -gt 24 ]]
  then
    echo "The name of the Terraform storage account is too long (>24).  Please reduce the length of the application + the tenant to 19 characters with no special characters."
    echo $TFSA_NAME
    echo $Name_Size
    exit 1
  else
    echo "${TFSA_NAME} passes length validation"  
  fi

  tmp_name="${svc_ppl_Name}${svc_ppl_TenantName}${svc_ppl_Environment}app"
  Name_Size=${#tmp_name}
  if [[ $Name_Size -gt 24 ]]
  then
    echo "The name of the Application storage account is too long (>24).  Please reduce the length of the application + the tenant to 19 characters with no special characters."
    echo $tmp_name
    echo $Name_Size
    exit 1  
  else
    echo "${tmp_name} passes length validation"     
  fi
}

function create_from_keyvault()
{
  # ============== CREATE TFVARS =================
  KEYVAULT_NAME="kv-${svc_ppl_Name}-${svc_ppl_TenantName}-${svc_ppl_Environment}"
  # store az info into variables
  export svc_ppl_TENANT_ID=$(echo $ACCOUNT | jq -r ".tenantId")
  export svc_ppl_SUB_ID=$(echo $ACCOUNT | jq -r ".id")

  export svc_ppl_CLIENT_SECRET=$(az keyvault secret show --vault-name $KEYVAULT_NAME --name SPTfClientSecret | jq -r ".value") #$(az ad sp create-for-rbac -n http://${svc_ppl_Name}-tf-sp-${svc_ppl_Environment} --query password -o tsv)
  export svc_ppl_CLIENT_ID=$(az keyvault secret show --vault-name $KEYVAULT_NAME --name SPTfClientId | jq -r ".value") #$(az ad sp show --id http://${svc_ppl_Name}-tf-sp-${svc_ppl_Environment} --query appId -o tsv)
  export svc_ppl_ACR_SP_SECRET=$(az keyvault secret show --vault-name $KEYVAULT_NAME --name SPAcrClientSecret | jq -r ".value") #$(az ad sp create-for-rbac --skip-assignment -n http://${svc_ppl_Name}-acr-sp-${svc_ppl_Environment} --query password -o tsv)
  export svc_ppl_ACR_SP_ID=$(az keyvault secret show --vault-name $KEYVAULT_NAME --name SPAcrClientId | jq -r ".value") #$(az ad sp show --id http://${svc_ppl_Name}-acr-sp-${svc_ppl_Environment} --query appId -o tsv)
  # export svc_ppl_GRAPH_SP_SECRET=$(az keyvault secret show --vault-name $KEYVAULT_NAME --name graphAppClientSecret | jq -r ".value") #$(az ad sp create-for-rbac --skip-assignment -n http://${svc_ppl_Name}-graph-${svc_ppl_Environment} --query password -o tsv)
  # export svc_ppl_GRAPH_SP_ID=$(az keyvault secret show --vault-name $KEYVAULT_NAME --name graphAppClientId | jq -r ".value") #$(az ad sp show --id http://${svc_ppl_Name}-graph-${svc_ppl_Environment} --query appId -o tsv)

  if [ $NO_CLOBBER -eq 0 ] 
  then
    # create terraform.tfvars and replace template values
    cat example.tfvars | \
      sed "s|<<svc_ppl_Name>>|$svc_ppl_Name|" | \
      sed "s|<<svc_ppl_ShortName>>|$svc_ppl_ShortName|" | \
      sed "s|<<svc_ppl_Location>>|$svc_ppl_Location|" | \
      sed "s|<<svc_ppl_Environment>>|$svc_ppl_Environment|" | \
      sed "s|<<svc_ppl_TenantName>>|$svc_ppl_TenantName|" | \
      sed "s|<<svc_ppl_Repo>>|$svc_ppl_Repo|" | \
      sed "s|<<svc_ppl_TENANT_ID>>|$svc_ppl_TENANT_ID|" | \
      sed "s|<<svc_ppl_SUB_ID>>|$svc_ppl_SUB_ID|" | \
      sed "s|<<svc_ppl_CLIENT_SECRET>>|$svc_ppl_CLIENT_SECRET|" | \
      sed "s|<<svc_ppl_CLIENT_ID>>|$svc_ppl_CLIENT_ID|" | \
      sed "s|<<svc_ppl_ACR_SP_SECRET>>|$svc_ppl_ACR_SP_SECRET|" | \
      sed "s|<<svc_ppl_ACR_SP_ID>>|$svc_ppl_ACR_SP_ID|" > terraform.tfvars
      # sed "s|<<svc_ppl_GRAPH_SP_ID>>|$svc_ppl_GRAPH_SP_ID|" | \
      # sed "s|<<svc_ppl_GRAPH_SP_SECRET>>|$svc_ppl_GRAPH_SP_SECRET|" \

    echo -e "${green}\tterraform.tfvars created${reset}"

    fi
}

# TODO: the secrets should be pushed into KeyVault by this script *not* by terraform.
function create_new_deployment()
{
  # ============== CREATE TFVARS =================
  # store az info into variables
  export svc_ppl_TENANT_ID=$(echo $ACCOUNT | jq -r ".tenantId")
  export svc_ppl_SUB_ID=$(echo $ACCOUNT | jq -r ".id")
  export svc_ppl_CLIENT_SECRET=$(az ad sp create-for-rbac -n http://${svc_ppl_Name}-sp-${svc_ppl_Environment} --query password -o tsv)
  export svc_ppl_CLIENT_ID=$(az ad sp show --id http://${svc_ppl_Name}-sp-${svc_ppl_Environment} --query appId -o tsv)
  export svc_ppl_ACR_SP_SECRET=$(az ad sp create-for-rbac --skip-assignment -n http://${svc_ppl_Name}-acr-sp-${svc_ppl_Environment} --query password -o tsv)
  export svc_ppl_ACR_SP_ID=$(az ad sp show --id http://${svc_ppl_Name}-acr-sp-${svc_ppl_Environment} --query appId -o tsv)
  # export svc_ppl_GRAPH_SP_SECRET=$(az ad sp create-for-rbac --skip-assignment -n http://${svc_ppl_Name}-graph-${svc_ppl_Environment} --query password -o tsv)
  # export svc_ppl_GRAPH_SP_ID=$(az ad sp show --id http://${svc_ppl_Name}-graph-${svc_ppl_Environment} --query appId -o tsv)

  # create terraform.tfvars and replace template values
  cat example.tfvars | \
    sed "s|<<svc_ppl_Name>>|$svc_ppl_Name|" | \
    sed "s|<<svc_ppl_ShortName>>|$svc_ppl_ShortName|" | \
    sed "s|<<svc_ppl_Location>>|$svc_ppl_Location|" | \
    sed "s|<<svc_ppl_Environment>>|$svc_ppl_Environment|" | \
    sed "s|<<svc_ppl_TenantName>>|$svc_ppl_TenantName|" | \
    sed "s|<<svc_ppl_Repo>>|$svc_ppl_Repo|" | \
    sed "s|<<svc_ppl_TENANT_ID>>|$svc_ppl_TENANT_ID|" | \
    sed "s|<<svc_ppl_SUB_ID>>|$svc_ppl_SUB_ID|" | \
    sed "s|<<svc_ppl_CLIENT_SECRET>>|$svc_ppl_CLIENT_SECRET|" | \
    sed "s|<<svc_ppl_CLIENT_ID>>|$svc_ppl_CLIENT_ID|" | \
    sed "s|<<svc_ppl_ACR_SP_SECRET>>|$svc_ppl_ACR_SP_SECRET|" | \
    sed "s|<<svc_ppl_ACR_SP_ID>>|$svc_ppl_ACR_SP_ID|" > terraform.tfvars
    
    # sed "s|<<svc_ppl_GRAPH_SP_ID>>|$svc_ppl_GRAPH_SP_ID|" | \
    # sed "s|<<svc_ppl_GRAPH_SP_SECRET>>|$svc_ppl_GRAPH_SP_SECRET|" \

  echo -e "${green}\tterraform.tfvars created${reset}"

  # ============== CREATE RESOURCES =================
  # Grant Application.ReadWrite.All and Directory.Read.All API access to Service Principal (${svc_ppl_Name}-tf-sp)
  # Get service principal App ID
  export servicePricipalId=$svc_ppl_CLIENT_ID #$(az ad sp list --query "[?appDisplayName=='${svc_ppl_Name}-tf-sp-${svc_ppl_Environment}'].appId | [0]" --all) 
  servicePricipalId=$(eval echo $servicePricipalId)
  echo "Service Principal AppID: " $servicePricipalId

  # Get MSGraphId
  export graphId=$(az ad sp list --query "[?appDisplayName=='Microsoft Graph'].appId | [0]" --all) 
  graphId=$(eval echo $graphId)
  echo "Service MSGraph AppID: " $graphId


  # Get MSGraph Permission variables
  export appReadWriteAll=$(az ad sp show --id $graphId --query "oauth2Permissions[?value=='Application.ReadWrite.All'].id | [0]") 
  appReadWriteAll=$(eval echo $appReadWriteAll)
  echo "Application.ReadWrite.All ID: " $appReadWriteAll


  export dirReadAll=$(az ad sp show --id $graphId --query "oauth2Permissions[?value=='Directory.Read.All'].id | [0]") 
  dirReadAll=$(eval echo $dirReadAll)
  echo "Directory.Read.All ID:" $dirReadAll

  # Add App persmission 
  az ad app permission add --id $servicePricipalId --api $graphId --api-permissions $dirReadAll=Scope $appReadWriteAll=Scope

  # Make permissions effective
  az ad app permission grant --id $servicePricipalId --api $graphId

  # Admin consent
  az ad app permission admin-consent --id $servicePricipalId

  ## Get graph service principal App ID--------------------------------------------------------------------------------------------

  # export graphServicePrincipalId=$svc_ppl_GRAPH_SP_ID
  # servicePricipalId=$(eval echo $graphServicePrincipalId)
  # echo "Graph Service Principal AppID: " $graphServicePrincipalId

  # export appRoleAppReadWriteAll=$(az ad sp show --id $graphId --query "appRoles[?value=='Application.ReadWrite.All'].id | [0]") 
  # appRoleAppReadWriteAll=$(eval echo $appRoleAppReadWriteAll)
  # echo "Application- Application.ReadWrite.All ID: " $appRoleAppReadWriteAll


  # export appRoleDirReadAll=$(az ad sp show --id $graphId --query "appRoles[?value=='Directory.Read.All'].id | [0]") 
  # appRoleDirReadAll=$(eval echo $appRoleDirReadAll)
  # echo "Application- Directory.Read.All ID:" $appRoleDirReadAll


  # # Add App permission 
  # az ad app permission add --id $graphServicePrincipalId --api $graphId --api-permissions $appRoleDirReadAll=Role $appRoleAppReadWriteAll=Role

  # # Make permissions effective
  # az ad app permission grant --id $graphServicePrincipalId --api $graphId

  # # Admin consent
  # az ad app permission admin-consent --id $graphServicePrincipalId




  # create tf_state resource group
  export TF_RG_NAME=rg-${svc_ppl_Name}-${svc_ppl_TenantName}-${svc_ppl_Environment}-tf
  echo "Creating the Deployment Resource Group"
  if echo ${TF_RG_NAME} > /dev/null 2>&1 && echo ${svc_ppl_Location} > /dev/null 2>&1; then
      if ! az group create --name ${TF_RG_NAME} --location ${svc_ppl_Location} -o table; then
          echo "ERROR: failed to create the resource group"
          exit 1
      fi
      echo "Created Resource Group: ${TF_RG_NAME} in ${svc_ppl_Location}"
  fi

  # create storage account for state file
  export TFSUB_ID=$(az account show -o tsv --query id)

  # STORAGE ACCOUNT NAME IS BUILT IN validate_environment
  tmp_name="citfstate"
  export TFCI_NAME=${tmp_name,,}

  echo "Creating Deployment Storage Account and State Container"


  if echo ${TFSUB_ID} > /dev/null 2>&1; then
      if ! az storage account create --resource-group $TF_RG_NAME --name $TFSA_NAME --sku Standard_LRS --encryption-services blob -o table; then
          echo "ERROR: Failed to create Storage Account"
          exit 1
      fi
      echo "Storage Account Created."
      sleep 20s
  fi

  # retrieve storage account access key
  if echo ${TF_RG_NAME} > /dev/null 2>&1; then
      if ! ARM_ACCESS_KEY=$(az storage account keys list --resource-group $TF_RG_NAME --account-name $TFSA_NAME --query [0].value -o tsv); then
          echo "ERROR: Failed to Retrieve Storage Account Access Key"
          exit 1
      fi
      echo "Storage Account Access Key = $ARM_ACCESS_KEY"
  fi




  if echo ${TF_RG_NAME} > /dev/null 2>&1; then
      if ! az storage container create --name $TFCI_NAME --account-name $TFSA_NAME --account-key $ARM_ACCESS_KEY -o table; then
          echo "ERROR: Failed to Retrieve Storage Container"
          exit 1
      fi
      echo "TF State Storage Account Container Created"
      export TFSA_CONTAINER=$(az storage container show --name ${TFCI_NAME} --account-name ${TFSA_NAME} --account-key ${ARM_ACCESS_KEY} --query name -o tsv)
      echo "TF Storage Container name = ${TFSA_CONTAINER}"
  fi

}

############################### MAIN ###################################
parse_args "$@"

validate_environment

if [ $NO_CLOBBER -eq 0 ]
then
  if [ $FIRST_RUN -eq 0 ]
  then
    create_from_keyvault
  else
    confirm_action "This will create new infrastructure.  Are you sure?"
    create_new_deployment
  fi
fi
