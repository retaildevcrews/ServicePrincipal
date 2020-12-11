# Introduction
The infrastructure is provision using Terraform, by Hashicorp.  The provisioning process in comprised of an variable file creation and initial resource creation (ResourceGroup, StorageAccount, ServicePrincipals). 

The ResourceGroup is used to contain resources specific to this application.  

The StorageAccount is used by Terraform to store its state file.  This state file is a critical resource that if lost, the infrastructure would need to be recreated.  

There are three ServicePrincipals created as part of the provisioning process:

ServicePrincipal | Purpose | Permissions
-----------------|---------|---------
 <appName>-tf-sp-<env> | Application Identity | Application.ReadWrite.All  Directory.Read.All
 A2 | B2 | C2
 A3 | B3 | C3

# PREREQUISITES
* An Azure subscription in which you have administrator access
* Administrator access to the Azure subscription directory (AAD)
* A local install of Terraform (<= v0.13.5) (https://learn.hashicorp.com/tutorials/terraform/install-cli)  
On **Ubuntu**, a bash script has been provided to install Terraform (```./install-terraform.sh```)
* A Linux command line running bash.  This may be a Linux Docker container, VM, WSL or bare metal machine.

>The following walkthrough requires you to be in the infra directory

## Login to Azure
```bash
az login -u login@yourdomain.com

###  show your Azure accounts
az account list -o table

###  select the Azure subscription if necessary
az account set -s {subscription name or Id}
```


# The Provisioning Script
The provisioning script is as Bash script and must be run from either a Linux machine or a Linux container.  WSL may also be used if using a Windows machine.  The script is used to automate the commands for Terraform and Terraform variable creation.  It helps to prevent errors in issuing the Terraform commands that could have a material impact on the infrastructure state.

Run the command
```bash
./provision-environment.sh --help
```
You will see the output:
```bash
usage: ./provision-environment.sh
          -h|--help
          -a|--appname <applicationname>
          -e|--env qa|dev
               environment for deployment
          -f|--first-run
               create a new tfvars. If this is not set, attempt to recreate tfvars from keyvault
          -i|--init
               force initialization of terraform. By default if .terraform directory exists it will not be overwritten.
          -l|--location <azure location>
          -r|--repo <repository name>
             --what-if
```

Argument | Description | Default
---------|-------------|--------
 appName | The long name of the application. The value must be between 5-18 characters long. | 
 env | The short name of the environment type being deployed, e.g. dev, prod | dev
 first-run | Indicate the first 'create' run or a secondary 'update' run.  A first run (re)creates the resources for Terraform. | False (not present)
 init | Force the initialize of terraform.  This should only be done on first run or in a new workspace | False (not present)
 location | The Azure region for resource creation | centralus
 repo | Name of the respository in the Azure Container Registry for the application container | Value of appName

The Terraform script needs a number of variables to be set in order to properly provision the infrastructure.  The script is structured so that all the variables are initialized in a file called `terraform.tfvars`.

### First Run
On first run, a ResourceGroup and StorageAccount will be created for the application and Terraform state management.  Additionally, the terraform.tfvars file will be created as input into the Terraform infrastructure code.  The Terraform code will push information about the created 
### Choose a unique DNS name

```bash
# this will be the prefix for all resources
#  only use a-z and 0-9 - do not include punctuation or uppercase characters
#  must be at least 5 characters long
#  must start with a-z (only lowercase)
export svc_ppl_Name=[your unique name]

### check if accounts exists globally
az storage account check-name -n ${svc_ppl_Name}
az cosmosdb check-name-exists -n ${svc_ppl_Name}
az acr check-name -n ${svc_ppl_Name}

### if accounts exists globally check if accounts exist in subscription
### returns 1 if exists
az storage account list --query "[?name=='${svc_ppl_Name}']" -o tsv | wc -l
az cosmosdb list --query "[?name=='${svc_ppl_Name}']" -o tsv | wc -l
az acr list --query "[?name=='${svc_ppl_Name}']" -o tsv | wc -l

### if 0 is returned, pick another name
```

### Set additional values

```bash
# export svc_ppl_Email=replaceWithYourEmail

### change the location (optional)
export svc_ppl_Location=centralus


###  >>>>>>>>>>>>>>>>>>>>>>>>> TODO <<<<<<<<<<<<<<<<<<<<<<<<<<
###  change the repo (optional - valid: test-csharp, test-java, test-typescript)
export svc_ppl_Repo=test-csharp
```

## Deploy serviceprincipal
Make sure you are in the serviceprincipal/infra directory 

```bash
### create tfvars file
./create-tf-vars.sh

###  initialize inline backend configuration
terraform init -backend-config="resource_group_name=${svc_ppl_Name}-rg-${svc_ppl_Enviroment}" -backend-config="storage_account_name=${svc_ppl_Name}st${svc_ppl_Enviroment}" -backend-config="container_name=${svc_ppl_Name}citfstate${svc_ppl_Enviroment}" -backend-config="key=${svc_ppl_Name}.terraform.tfstate.${svc_ppl_Enviroment}"

###  validate
terraform validate

###  If you have no errors you can create the resources
terraform apply -auto-approve

###  This generally takes about 10 minutes to complete

```

## Verify the deployment

```bash

# check the App Service
curl https://${svc_ppl_Name}.azurewebsites.net/version
curl https://${svc_ppl_Name}.azurewebsites.net/api/genres

# check the docker logs from the webv tests
az container logs -g ${svc_ppl_Name}-rg-webv -n ${svc_ppl_Name}-webv-${svc_ppl_Location}

# check Log Analytics
export svc_ppl_LogAnalytics_Id='az monitor log-analytics workspace show -g ${svc_ppl_Name}-rg-webv -n ${svc_ppl_Name}-webv-logs --query customerId -o tsv'

az monitor log-analytics query -w $(eval $svc_ppl_LogAnalytics_Id) --analytics-query "ContainerInstanceLog_CL | sort by TimeGenerated"

```

You can also log into the Azure portal and browse the new resources

> If the terraform plan command is redirected to a file, there will be secrets stored in that file!
>
> Be sure not to remove the ignore *tfplan* in the .gitignore file
>

## Module Documentation -- TODO

Each module has a `README` file in the module directory under [`./infra`](./infra). You can reference the module documentation for the specific requirements of each module.

The main calling Terraform script can be found at [`./main.tf`](./main.tf)

Customizations are generated by `create-tf-vars.sh` and stored in `terraform.tfvars`.  This file should never be added to github and is included in the .gitignore file.

## Removing the deployment

>
> WARNING - this will delete everything with only one prompt
>

```bash

# this takes several minutes to run
terraform destroy

# remove resource group and nested resources , this will delete Storage Account, Container and remote tfstate file 
az group delete --name ${svc_ppl_Name}-rg-${svc_ppl_Enviroment}

# delete the service principals
az ad sp delete --id http://${svc_ppl_Name}-acr-sp-${svc_ppl_Enviroment}
az ad sp delete --id http://${svc_ppl_Name}-tf-sp-${svc_ppl_Enviroment}
az ad sp delete --id http://${svc_ppl_Name}-graph-${svc_ppl_Enviroment}

# remove state and vars files
rm terraform.tfvars

```
