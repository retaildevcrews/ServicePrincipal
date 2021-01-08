#!/bin/bash



# USAGE:
#   provision-environment.sh
#       --appName   | -a <applicationname>
#       --location  | -l <location>
#       --env       | -e {qa|prod}
#       --repo      | -r <reponame>

# # TECHNIQUE 1
# while getopts “:a:bc” opt; do
#   case $opt in
#     a) AOPT=$OPTARG ;;
#   esac
# done

red=`tput setaf 1`
green=`tput setaf 2`
yellow=`tput setaf 3`
reset=`tput sgr0`
die() { echo -e "${red}$*${reset}" >&2; exit 2; }  # complain to STDERR and exit with error
ok() { echo -e "\t${green}$*${reset}";  }  # emit success message
fail() { echo -e "\t${red}$*${reset}";  }  # emit fail message

goodbye() { [ ! -z "${1##*[!0-9]*}" ] && exit $1 || exit 2; }  # complain to STDERR and exit with error
#needs_arg() { if [ -z "$OPTARG" ]; then die "No arg for --$OPT option"; fi; }

function parse_args()
{
    WHAT_IF=0
    APP_NAME=""
    ENV="dev"
    FIRST_RUN=0
    LOCATION="centralus"
    REPO=""
    PARAMS=""
    VALIDATE_ONLY=0
    INIT=0
    TENANT_NAME=""

    while (( "$#" )); do
      case "$1" in
        -h|--help)
            echo "${yellow}usage: $0" >&2
            echo "          -h|--help" >&2
            echo "          -a|--appname <applicationname>" >&2
            echo "          -e|--env qa|dev" >&2
            echo "               environment for deployment" >&2
            echo "          -f|--first-run" >&2
            echo "               create a new tfvars. If this is not set, attempt to recreate tfvars from keyvault" >&2
            echo "          -i|--init" >&2
            echo "               force initialization of terraform. By default if .terraform directory exists it will not be overwritten." >&2
            echo "          -l|--location <azure location>" >&2
            echo "          -t|--tenant-name <tenant name abbreviation>" >&2
            echo "          -r|--repo <repository name>" >&2
            # echo "          -v|--validate-only" >&2
            # echo "               do not try to apply terraform changes" >&2
            echo "             --what-if${reset}" >&2
            goodbye 1
            ;;
        (--what-if)
          WHAT_IF=1
          shift
          ;;
        -a|--appname)
          if [ -n "$2" ] && [ ${2:0:1} != "-" ]; then
            APP_NAME=$2
            shift 2
          else
            die "Error: Argument for $1 is missing"
          fi
          ;;
        -e|--env)
          if [ -n "$2" ] && [ ${2:0:1} != "-" ]; then
            ENV=$2
            shift 2
          else
            die "Error: Argument for $1 is missing"
          fi
          ;;
        -f|--first-run)
          FIRST_RUN=1
          shift 1
          ;;      
        -i|--init)
          INIT=1
          shift 1
          ;;                
        -l|--location)
          if [ -n "$2" ] && [ ${2:0:1} != "-" ]; then
            LOCATION=$2
            shift 2
          else
            die "Error: Argument for $1 is missing"
          fi
          ;;      
        -t|--tenant-name)
          if [ -n "$2" ] && [ ${2:0:1} != "-" ]; then
            TENANT_NAME=$2
            shift 2
          else
            die "Error: Argument for $1 is missing"
          fi
          ;;      
        -r|--repo)
          if [ -n "$2" ] && [ ${2:0:1} != "-" ]; then
            REPO=$2
            shift 2
          else
            die "Error: Argument for $1 is missing"
          fi
          ;;      
        -*|--*=) # unsupported flags
          die "Error: Unsupported flag $1"
          ;;
        *) # preserve positional arguments
          PARAMS="$PARAMS $1"
          shift
          ;;
      esac
    done
    # set positional arguments in their proper place
    eval set -- "$PARAMS"

    # Validate/Default arguments
    Name_Size=${#APP_NAME}
    if [[ $Name_Size -lt 5 || $Name_Size -gt 18 ]]
    then
      echo "${red}Please appName must be between 5 and 18 characters in length with no special characters."
      echo "'$APP_NAME': $Name_Size${reset}"
      die
    fi

    Name_Size=${#TENANT_NAME}
    if [[ $Name_Size -lt 1 || $Name_Size -gt 5 ]]
    then
      echo "${red}Tenant name must be between 1 and 5 characters in length with no special characters."
      echo "'$TENANT_NAME': $Name_Size${reset}"
      die
    fi

    if [ -z $REPO ]
    then
      die "Repository not specified."
    fi

    echo ""
    echo -e "${yellow}WHAT_IF: \t$WHAT_IF${reset}"
    echo -e "FIRST_RUN: \t$FIRST_RUN"
    echo -e "APP_NAME: \t$APP_NAME"
    echo -e "TENANT_NAME: \t$TENANT_NAME"
    echo -e "ENV: \t\t$ENV"
    echo -e "LOCATION: \t$LOCATION"
    echo -e "REPO: \t\t$REPO"    
    echo ""
}

# $1 = prompt text 
# $2 = if response is Nn, =0 or missing exits, =1 returns 1
function confirm_action()
{
  action="(Yn) "
  prompt="${1} ${action}"
  while true; do
      read -p "$prompt" yn
      case $yn in
          [Y]* ) return 0;;
          [Nn]* ) 
            if [ -z $2 ] || [ $2 -eq 0 ]
            then
                exit 1
            else
                return 1
            fi
            ;;
          * ) echo "Please answer Y or n";;
      esac
  done
}

check_errors=()
function check_exists()
{
    if (eval "$2 --output tsv | grep $3") > /dev/null 2>&1
    then 
      msg="$1 '$3' already exists."
      check_errors+=("$msg")
    else 
      ok "$1 '$3' does not exist."
    fi  
}

function check_length()
{
    Name_Size=${#1}
    if [[ $Name_Size -gt $2 ]]
    then
      msg="The name is too long (>$2).\n${$1}: ${Name_Size}"
      check_errors+=("$msg")
    else
      ok "${1} passes length validation: ${Name_Size} < $2"  
    fi
}


function Build_Resource_Names()
{
  # the following two are needed by prepare, check them for existance and length
  export TFRG_NAME=$(./build-resource-name.sh -r resourcegroup -n $APP_NAME -e $ENV -t $TENANT_NAME)-tf
  export TFSA_NAME=$(./build-resource-name.sh -r storageaccount -n $APP_NAME -e $ENV -t $TENANT_NAME)tf
  export KEYVAULT_NAME=$(./build-resource-name.sh -r keyvault -n $APP_NAME -e $ENV -t $TENANT_NAME)   
  export TFCI_NAME=citfstate 

  # the following are created by terraform, check them for existance and length
  export APPSA_NAME=$(./build-resource-name.sh -r storageaccount -n $APP_NAME -e $ENV -t $TENANT_NAME)app
  export ACR_NAME=$(./build-resource-name.sh -r containerregistry -n $APP_NAME -e $ENV -t $TENANT_NAME)
  export FA_NAME=$(./build-resource-name.sh -r functionapp -n $APP_NAME -e $ENV -t $TENANT_NAME)
  export CDB_NAME=$(./build-resource-name.sh -r cosmosaccount -n $APP_NAME -e $ENV -t $TENANT_NAME)
}

# Check to see if the StorageAccount, ContainerRegistry, FunctionApp, CosmosDB and KeyVault are already present on first-run
function Check_Resource_Names()
{
  check_errors=()
  if [ $FIRST_RUN -eq 1 ]
  then

    # Check Length of TF Storage Account Name
    check_length $TFSA_NAME 24

    # Check Length of TF Storage Account Name
    check_length $APPSA_NAME 24

    ## Check for existence of "public" Application resources
    # STORAGE ACCOUNT
    check_exists "Storage Account" "az storage account list" $APPSA_NAME

    # CONTAINER REGISTRY
    check_exists "Container Registry" "az acr list" $ACR_NAME

    # FUNCTION APP
    check_exists "Function App" "az functionapp list" $FA_NAME

    # COSMOSDB
    check_exists "ComsmosDB" "az cosmosdb list" $CDB_NAME

    # KEYVAULT
    check_exists "KeyVault" "az keyvault list" $KEYVAULT_NAME   

    if [ ${#check_errors[@]} -gt 0 ] 
    then
      for i in "${check_errors[@]}"
      do
        fail $i
      done

      die "Cannot continue."
    fi
  fi
}

function Ensure_Azure_Login()
{
    ACCOUNT=$(az account show)
    if [ $? -eq 1 ]
    then
      die "Please login to Azure first"
    fi
}

function Setup_Global_Environment()
{
    export svc_ppl_Name=$APP_NAME
    export svc_ppl_Location=$LOCATION
    export svc_ppl_Environment=$ENV
    export svc_ppl_Repo=$REPO
    export svc_ppl_TenantName=$TENANT_NAME

    Build_Resource_Names
    Check_Resource_Names
}


function Prepare_Terraform()
{
    ARGS=()
    if [ -f "terraform.tfvars" ]
    then
        confirm_action "terraform.tfvars exists, overwrite?" 1
        if [ $? -eq 1 ]
        then
            ARGS=("-n") # NO CLOBBER
        fi
    fi

    echo "${yellow}Calling prepare-terraform${reset}"
    if [ $WHAT_IF -eq 1 ]
    then
        echo -e "${yellow}\tCalling prepare-terraform with what-if${reset}"
        ARGS+=("-w")
    fi
    if [ $FIRST_RUN -eq 1 ]
    then
        echo -e "${yellow}\tCalling prepare-terraform with first-run${reset}"
        ARGS+=("-f")
    fi

    source ./_prepare-terraform.sh "${ARGS[@]}"
}

function Prepare_Environment()
{
    Ensure_Azure_Login
    Setup_Global_Environment
    Prepare_Terraform
}

function Initialize_Terraform()
{
    if [ ! -d ./.terraform ] || [ $INIT -eq 1 ]
    then
      # The TF variables are initialized in _prepare-terraform
        terraform init -reconfigure -backend-config="resource_group_name=${TFRG_NAME}" -backend-config="storage_account_name=${TFSA_NAME}" -backend-config="container_name=${TFCI_NAME}" -backend-config="key=${svc_ppl_Name}.terraform.tfstate"
    fi
}

function Validate_Terraform()
{
    terraform validate
}

############################### MAIN ###################################

parse_args "$@"

Prepare_Environment

Initialize_Terraform
Validate_Terraform

