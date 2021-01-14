![License](https://img.shields.io/badge/license-MIT-green.svg)
<h1>Application Configuration</h1>
This document discusses the configuration strategy and configuration settings for each environment type.  
    
- [Overview](#overview)
- [Environments](#environments)
- [Configuration Sources](#configuration-sources)
- [Configuration Settings](#configuration-settings)
- [Local Development Environment](#local-development-environment)
- [Shared Environment](#shared-environment)
  - [Shared Development Environment](#shared-development-environment)
  - [Shared QA Environment](#shared-qa-environment)
- [Production Environment](#production-environment)


## Overview
Configuration values are segregated into several categories:  
* __Static__ - values that credential related metadata, including connection strings with embedded credential information.  In a production environment, Secrets are typically managed to a separate team (other than the development/support team) due to SOX Audit requirements.
* __Non Secrets__ - values that are the largest population of configuration values that do no include credential information and allow behavior of the application to be changed as a _deployment_ time concern.
* __Secrets__ - non-secret values that allow behavior of the application to be set as a _development_ time concern.

## Environments
 
Name | Code | Category | Description  
-----|------|--------- | -----------
 Local Dev | - | Local | Local Development Environment  
 Development | dev | Non-Production | Shared Development Environment (in Azure)  
 QA | qa | Non-Production | Testing Environment  
 Production | prod | Production | Production environment  

 > __Note__: We have made the decision __not__ to create a full separate QA environment.  We provision separate QA storage (StorageAccount, CosmosDB Database) to separate the data persistence needs of shared dev and qa.

## Configuration Sources
As we look at the path to production, the configuration needs change.  What is suitable for a local developer's machine is not appropriate for a production environment.

The configuration sources vary a litte between a local and non-local environment.  The sources are enumerated in the table below.

Source| Configuration Category | Environment Categories
---------|----------|---------
 Function Configuration | NonSecret | Local, Non-Production, Production
 appsettings.Development.json | NonSecret | Local, Non-Production
 appsettings.Production.json | NonSecret | Production
 KeyVault | Secret | Non-Production, Production


## Configuration Settings

Name                        | Description | Category | Source | DataType | Values | Default Value
----------------------------|-------------|----------|--------|----------|--------|--------------
 AUTH_TYPE                  | Authentication type  | NonSecret | Function Configuration / Environment | X | MI | CLI, MI, VS  
 KEYVAULT_NAME              | X | NonSecret | Function Configuration | string | - | Set by Terraform
 graphAppClientId           | X | Secret    | KeyVault | Guid | - | Set by Terraform  
 graphAppTenantId           | X | Secret    | KeyVault | Guid | - | Set by Terraform   
 graphAppClientSecret       | X | Secret    | KeyVault | string | - | Set by Terraform    
 SPCosmosKey                | X | Secret    | KeyVault | string | - | Set by Terraform   
 SPCosmosURL                | X | NonSecret | Function Configuration | string | - | Set by Terraform   
 SPCosmosDatabase           | X | NonSecret | Function Configuration | string | - | Set by Terraform   
 SPConfigurationCollection  | X | NonSecret | Function Configuration | string | - | Set by Terraform   
 SPAuditCollection          | X | NonSecret | Function Configuration | string | - | Audit   
 SPObjectTrackingCollection | X | NonSecret | Function Configuration | string | - | ObjectTracking   
 SPActivityHistoryCollection| X | NonSecret | Function Configuration | string | - | ActivityHistory   
 SPStorageConnectionString  | X | Secret    | KeyVault | string | - | Set by Terraform   
 SPDiscoverQueue            | X | NonSecret | Function Configuration | string | - | discover   
 SPEvaluateQueue            | X | NonSecret | Function Configuration | string | - | evaluate     
 SPUpdateQueue              | X | NonSecret | Function Configuration | string | - | update   
 SPDeltaDiscoverySchedule   | X | NonSecret | Function Configuration | string | - | 0 */30 * * * * <br />(See [NCHRON](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-timer?tabs=csharp#ncrontab-expressions))   
 configId                   | X | NonSecret | appSettings.X.json | Guid | - | 02a54ac9-441e-43f1-88ee-fde420db2559   
 aadUpdateMode              | X | NonSecret | Function Configuration | Enum | Update, ReportOnly | Update   
 visibilityDelayGapSeconds  | X | NonSecret | appSettings.X.json | X | - | 8   
 queueRecordProcessThreshold| X | NonSecret | appSettings.X.json | X | - | 10   


## Local Development Environment

## Shared Environment
### Shared Development Environment
### Shared QA Environment
## Production Environment
