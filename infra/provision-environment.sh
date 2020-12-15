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
    TENANT_NAME="cse"

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
        -a|--appName)
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
        # -v|--validate-only)
        #   VALIDATE_ONLY=1
        #   shift 1
        #   ;;      
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
    echo "appName: '$APP_NAME'"
    echo "appName length = $Name_Size${reset}"
    die
    fi

    Name_Size=${#TENANT_NAME}
    if [[ $Name_Size -lt 2 || $Name_Size -gt 4 ]]
    then
    echo "${red}Please appName must be between 2 and 4 characters in length with no special characters."
    echo "appName: '$TENANT_NAME'"
    echo "appName length = $Name_Size${reset}"
    die
    fi

    if [ -z $REPO ]
    then
    echo "${yellow}Repository not specified, defaulting to '$APP_NAME'.${reset}"
    REPO=$APP_NAME
    fi

    echo ""
    echo "${yellow}WHAT_IF: $WHAT_IF${reset}"
    echo "APP_NAME: $APP_NAME"
    echo "ENV: $ENV"
    echo "FIRST_RUN: $FIRST_RUN"
    echo "LOCATION: $LOCATION"
    echo "REPO: $REPO"    
    echo "TENANT_NAME: $TENANT_NAME"
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

function Ensure_Azure_Login()
{
    ACCOUNT=$(az account show)
    if [ $? -eq 1 ]
    then
      die "Please login to Azure first"
    fi
}

function Setup_Environment_Variables()
{
    export svc_ppl_Name=$APP_NAME
    export svc_ppl_Location=$LOCATION
    export svc_ppl_Environment=$ENV
    export svc_ppl_Repo=$REPO
    export svc_ppl_TenantName=$TENANT_NAME
}

function Setup_Terraform_Variables()
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
    Setup_Environment_Variables
    Setup_Terraform_Variables
}

function Initialize_Terraform()
{
    if [ ! -d ./.terraform ] || [ $INIT -eq 1 ]
    then
        terraform init -backend-config="resource_group_name=rg-${svc_ppl_Name}-${svc_ppl_Environment}-tf" -backend-config="storage_account_name=${svc_ppl_Name}${svc_ppl_Environment}tf" -backend-config="container_name=citfstate${svc_ppl_Environment}" -backend-config="key=${svc_ppl_Name}.terraform.tfstate"
    fi
}

function Validate_Terraform()
{
    terraform validate
}

# function Apply_Terraform()
# {
#     terraform apply --auto-approve
# }

############################### MAIN ###################################

parse_args "$@"

Ensure_Azure_Login

Prepare_Environment

Initialize_Terraform
Validate_Terraform

# if [ $VALIDATE_ONLY -eq 0 ]
# then
#     Apply_Terraform
# fi
