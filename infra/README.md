- [Introduction](#introduction)
- [Prerequisites](#prerequisites)
  - [Login to Azure](#login-to-azure)
- [The Provisioning Script](#the-provisioning-script)
  - [Resource Naming](#resource-naming)
  - [Deploy ServicePrincipal](#deploy-serviceprincipal)
    - [First Run](#first-run)
      - [Plan Validation](#plan-validation)
    - [Second Run](#second-run)
- [Verify the deployment](#verify-the-deployment)
- [Removing the deployment](#removing-the-deployment)
  
# Introduction
The infrastructure is provisioned using Terraform, by Hashicorp.  The provisioning process is comprised of a Terraform variable file and initial resources (ResourceGroup, StorageAccount, ServicePrincipals) created via CLI.

The ResourceGroup is used to contain resources specific to this application.  

The StorageAccount is used by Terraform to store its state file.  This state file is a critical resource that if lost, the infrastructure would need to be recreated.  

There are two ServicePrincipals created as part of the provisioning process:

ServicePrincipal | Purpose | Graph Permissions | Resource Permissions
-----------------|---------|-------------------|-----------
 \<appName>-sp-\<env> | Application Identity | Application.ReadWrite.All  Directory.Read.All | 
 \<appName>-acr-sp-\<env> | Container Registry Identity | | Container Registry - pull

The service principal \<appName>-sp-\<env> requires the permissions listed in the table below to enable Microsoft Graph API calls that support the major units of work.

| API Permission | Permission Type | MS Graph API Call | Documentation |
| --- | --- | --- | --- |
| Directory.Read.All  | Delegated | GET ServicePrincipal | https://docs.microsoft.com/en-us/graph/api/serviceprincipal-get?view=graph-rest-beta&tabs=http |
| | | LIST ServicePrincipals | https://docs.microsoft.com/en-us/graph/api/serviceprincipal-list?view=graph-rest-beta&tabs=http |
| Application.ReadWrite.All  | Delegated | UPDATE ServicePrincipal | https://docs.microsoft.com/en-us/graph/api/serviceprincipal-update?view=graph-rest-beta&tabs=http |
| Directory.Read.All  | Application | GET ServicePrincipal | https://docs.microsoft.com/en-us/graph/api/serviceprincipal-get?view=graph-rest-beta&tabs=http |
| | | LIST ServicePrincipals | https://docs.microsoft.com/en-us/graph/api/serviceprincipal-list?view=graph-rest-beta&tabs=http |
| Application.ReadWrite.All  | Application | UPDATE ServicePrincipal | https://docs.microsoft.com/en-us/graph/api/serviceprincipal-update?view=graph-rest-beta&tabs=http |

A role assignment scoped to the azure container registry is created and assigned to the service principal \<appName>-acr-sp-\<env>.
 
# Prerequisites
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

The general flow of Terraform deployment is:
1. **terraform init** - done once for a new workspace
2. **terraform validate** - validates all the .tf scripts in the current directory and any referenced .tf child files
3. **terraform plan** - provides a 'what-if' view of what changes would be applied to the environment given its current state
4. **terraform apply** - performs the actions in the script and applies the changes to the infrastructure

The script will perform the following steps:  
1. Create / Recreate the ```terraform.tfvars``` file
2. On First-Run, create 
   * ServicePrincipal
   * Storage Account
   * KeyVault
3. Validate the Terraform Script (terraform validate)


Run the command
```bash
./provision-environment.sh --help
```
You will see the output:
```
usage: ./provision-environment.sh
          -h|--help
          -a|--appname <applicationname> (required)
          -e|--env qa|dev (default: dev)
               environment for deployment
          -f|--first-run
               create a new tfvars. If this is not set, attempt to recreate tfvars from keyvault
          -i|--init
               force initialization of terraform. By default if .terraform directory exists it will not be overwritten.
          -l|--location <azure location> (default: centralus)
          -t|--tenant-name <tenant name abbreviation> (required)
          -r|--repo <repository name> (required)
             --what-if

```

Argument | Description | Default
---------|-------------|--------
 appName | The long name of the application. The value must be between 5-18 characters long. | 
 env | The short name of the environment type being deployed, e.g. dev, prod | dev
 first-run | Indicate the first 'create' run or a secondary 'update' run.  A first run (re)creates the resources for Terraform. | False (not present)
 init | Force the initialize of terraform.  This should only be done on first run or in a new workspace | False (not present)
 location | The Azure region for resource creation (```az account list-locations -o table```)| centralus
 repo | Name of the respository in the Azure Container Registry for the application container | Value of appName

>The values of appname, env, tenant-name are combined to name the resources in the infrastructure.  For Storage Accounts in particular there is a max length contraint of 24 characters.  The script will check if there is a name length constraint violation for storage accounts.

The Terraform script needs a number of variables to be set in order to properly provision the infrastructure.  The script is structured so that all the variables are initialized in a file called `terraform.tfvars`.

## Resource Naming
Some of the resources that are created must have a globally unique name whether or not those resources are within a private network or not.  Those resources include StorageAccount, KeyVault, Function App, and CosmosDB Account.  In order to make them unique, you must choose values that, when composed into the resource name, will result in a globally unique name.  All the resources use this naming technique.  

For example, the KeyVault will be named using the following format ```kv-<appName>-<tenantName>-<environment>```.  If the substituion values are:

token | value 
---------|------- 
 appName | MyProject 
 tenantName | xyz 
 environment | dev 

the KeyVault will be named ```kv-myproject-xyz-dev```.

> Some resources must be all lower case.  The script will take care of case sensitivity as well.


## Deploy ServicePrincipal
>Make sure you are in the serviceprincipal/infra directory.  

The script will **not** perform a terraform apply.  This step must be performed manually to prevent any premature infrastructure changes.  

### First Run
On first run, a ResourceGroup and StorageAccount will be created for the application and Terraform state management.  Additionally, the terraform.tfvars file will be created as input into the Terraform infrastructure code.  The Terraform code will push information about the created identies and resources into the KeyVault for the application.

The provision-environment script will check the names for 1) length and 2) public uniqueness.  If there is any constraint violation, the script will report an error.  If this happens trying changing the application name and/or the tenant.  Remember the tenant should be a short abbreviation to help scope the resource names.  There are only a limited number of characters available.

In the example below, appname=**mysp**.  You should substitue your own unique value determined above in the DNS search.

```bash
### (Optional) You may need to make the scripts executable
chmod u+x ./*.sh

### provision the environment
./provision-environment.sh --init --first-run --location centralus --appname mysp --repo serviceprincipal --tenant-name lab
```
A shortened version of the command line is below.
```bash
### provision the environment
./provision-environment.sh -i -f -l centralus -a mysp -r serviceprincipal -t lab
```

You should see the message  
<span style="color:green">Success!</span> The configuration is valid.  
at the end of the output.

####  Plan Validation
At this point it is wise to perform a Terraform plan.  This will show you what Creates, Deletes, Updates will be applied to the infrastructure but without making any changes.

```bash
terraform plan
### A long output will be generated show all the resources and settings that will be affected.
```
Go through the output and verify the changes being proposed are in line with your expectations.

```bash
terraform apply 
###  This generally takes about 10 minutes to complete
```

### Second Run
This is a very similar process to the First Run except for the arguments you call ```./provision-environment.sh```.  This will not try to create the Terraform ResourceGroup, StorageAccount, or ServicePrincipal(s).  You may get prompted to overwrite terraform.tfvars.  Usually select **no** unless you know you want to generate a new variables file.  Otherwise the variables file will be recreated from the values in KeyVault.

>**terraform.tfvars** contains secrets and should never be checked into source code control. After infrastructure provisioning, this file should be removed.

```bash
### provision the environment
./provision-environment.sh --location centralus --appname mysp --repo serviceprincipal --tenant-name lab
```
A shortened version of the command line is below.
```bash
### provision the environment
./provision-environment.sh -l centralus -a mysp -r serviceprincipal -t lab
```
# Verify the deployment

Log into the Azure portal and browse the new resources

> If the terraform plan command is redirected to a file, there will be secrets stored in that file!
>
> Be sure not to remove the ignore *tfplan* in the .gitignore file
>

# Removing the deployment

>
> WARNING - this will delete everything with only one prompt
>

```bash
# this takes several minutes to run
terraform destroy

# *** You need the values of appname, tenantname and environment for the commands below ***

# remove resource group and nested resources , this will delete Storage Account, Container and remote tfstate file 
az group delete --name rg-<appname>-<tenantname>-<environment>-tf

# delete the service principals
az ad sp delete --id http://<appname>-sp-<environment>
az ad sp delete --id http://<appname>-acr-sp-<environment>
```
