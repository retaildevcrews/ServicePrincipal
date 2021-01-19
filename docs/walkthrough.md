![License](https://img.shields.io/badge/license-MIT-green.svg)
# Service Principal Developer Walkthrough 
- [Service Principal Developer Walkthrough](#service-principal-developer-walkthrough)
  - [Introduction](#introduction)
    - [Pre-requisites](#pre-requisites)
    - [Environment Setup](#environment-setup)

## Introduction

The goal of this document is to explain enough about the framework and system to help one get started with development.


### Pre-requisites

* [Install .NET Core 3.1](https://dotnet.microsoft.com/download/dotnet-core/3.1)
* [Visual Studio 2019](https://visualstudio.microsoft.com/downloads/)
* [Powershell v5 or higher](https://github.com/PowerShell/PowerShell/releases)
* [Azure PowerShell module](https://docs.microsoft.com/en-us/powershell/azure/install-az-ps?view=azps-5.0.0#install-the-azure-powershell-module)
* [Docker](https://docs.docker.com/get-docker/)
* [Azure Cosmos DB Emulator](https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator?tabs=cli%2Cssl-netstd21)
* [Microsoft Azure Storage Explorer](https://azure.microsoft.com/en-us/features/storage-explorer/)
  
### Environment Setup
* Create a project in Visual Studio
* Clone this repository in your project directory
* [Provision infrastructure](https://github.com/retaildevcrews/ServicePrincipal/blob/main/infra/README.md)
* [Set up CI/CD](https://github.com/retaildevcrews/ServicePrincipal/blob/main/docs/CICD_Walkthrough.md)
* Update local settings
  - Navigate to src/Automation/CSE.Automation
  - Create a copy of the file **local.settings.json.example** and name it **local.settings.json**
  - In the file **local.settings.json**, update or add the variables shown below with the listed values

    | Property | Value |
    |----------|-------|
    | KEYVAULT_NAME | < name of the key vault generated when infrastructure was provisioned in the previous step > |
    | AUTH_TYPE | "CLI" |
    | SPCosmosURL | "https://localhost:8081" |
    | SPCosmosDatabase  | "SPAutomate" |
    | SPStorageConnectionString | < get Connection String from your Microsoft Azure Storage Explorer under "Properties" > |

  - Navigate to src/Automation/SolutionScripts
  - In the file **local.settings.json.template**, update the value of the variable "KEYVAULT_NAME" with the name of the key vault generated when infrastructure was provisioned 
* Update app configuration settings for testing
  - Navigate to src/Automation/CSE.Automation.Tests
  - In the file **appconfig.json**, update the value of the variable "KEYVAULT_NAME" with the name of the key vault generated when infrastructure was provisioned
  - Navigate to src/Automation/CSE.Automation.TestsPrep
  - In the file **appconfig.json**, update the value of the variable "KEYVAULT_NAME" with the name of the key vault generated when infrastructure was provisioned