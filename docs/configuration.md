![License](https://img.shields.io/badge/license-MIT-green.svg)
<h1>Application Configuration</h1>
This document discusses the configuration strategy and configuration settings for each environment type.  
    
- [Overview](#overview)
- [Environments](#environments)
- [Configuration Sources](#configuration-sources)
- [Local Development Environment](#local-development-environment)
- ['Production' Environment](#production-environment)
- [Configuration Settings](#configuration-settings)


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

 > __Note__: We have made the decision __not__ to create a full separate QA environment.  We provision separate QA storage (StorageAccount, CosmosDB Database) to separate the data persistence needs of shared dev and qa.  Currently Integration Tests are run from the build client against QA storage resources.

## Configuration Sources
As we look at the path to production, the configuration needs change.  What is suitable for a local developer's machine is not appropriate for a production environment.

The configuration sources vary a litte between a local and non-local environment.  The sources are enumerated in the table below.

Source| Configuration Category | Environment Categories | Note
---------|----------|---------|-
 Function Configuration | NonSecret | Local, Non-Production, Production |This is either Environment variables or local.settings.json which are exposed as the Function Configuration in the Portal
 appsettings.Development.json | NonSecret | Local, Non-Production | Used to provide local dev environment overrides
 appsettings.Production.json | NonSecret | Production
 KeyVault | Secret | Non-Production, Production | Only secrets should be in the KeyVault, it is not a general purpose configuration store.


## Local Development Environment
The local development environment uses local.settings.json, environment variables, appSettings.Development.json, host.json and KeyVault for configuration sources.  The order of precedence of settings is as follows:
1. environment variables
2. local.settings.json
3. KeyVault
4. appSettings.Development.json

Where higher numbered sources overlay lower numbered sources.  
> The settings that are marked as Secret, will only be retrieved from KeyVault.

The local development environment is running the Azure Storage emulator and Azure CosmosDB emulator so local settings overrides are needed to point to the correct resources.  You should not use local.settings.json for these settings (locally) since this file is used for local development **as well as** the values for the function deployment into Azure.

## 'Production' Environment
The production environment uses Function Configuration, appSettings.Production.json, host.json and KeyVault for configuration sources.

## Configuration Settings

Name                        | Description | Category | Source | DataType | Values | Default Value
----------------------------|-------------|----------|--------|----------|--------|--------
 AUTH_TYPE                  | Authentication type for Azure resource access | NonSecret | Function Configuration / Environment | X | MI | CLI, MI, VS  
 KEYVAULT_NAME              | Name of the keyvault where configuration secrets are stored | NonSecret | Function Configuration | string | - | Set by Terraform
 graphAppClientId           | Client Id used to connect to the Graph API | Secret    | KeyVault | Guid | - | Set by Terraform  
 graphAppTenantId           | Tenant Id use to connect to the Graph API | Secret    | KeyVault | Guid | - | Set by Terraform   
 graphAppClientSecret       | Client Secret used to connect to the Graph API | Secret    | KeyVault | string | - | Set by Terraform    
 SPCosmosKey                | Secret used to connect to CosmosDB | Secret    | KeyVault | string | - | Set by Terraform   
 SPCosmosURL                | URI of CosmosDB instance | NonSecret | Function Configuration | string | - | Set by Terraform   
 SPCosmosDatabase           | Name of the CosmosDB instance | NonSecret | Function Configuration | string | - | Set by Terraform   
 SPConfigurationCollection  | Name of the collection in CosmosDB containing processor configuration | NonSecret | Function Configuration | string | - | Set by Terraform   
 SPAuditCollection          | Name of the collection in CosmosDB containing Audit records | NonSecret | Function Configuration | string | - | Audit   
 SPObjectTrackingCollection | Name of the collection in CosmosDB containing the Last Known Good (LKG) state of ServicePrincipals (non-authoritative) | NonSecret | Function Configuration | string | - | ObjectTracking   
 SPActivityHistoryCollection| Name of the collection in CosmosDB containing activity tracking history | NonSecret | Function Configuration | string | - | ActivityHistory   
 SPStorageConnectionString  | Connection string to connect to Storage Account (for Queues) | Secret    | KeyVault | string | - | Set by Terraform   
 SPDiscoverQueue            | Name of the queue for Discover messages | NonSecret | Function Configuration | string | - | discover   
 SPEvaluateQueue            | Name of the queue for Evaluate messages | NonSecret | Function Configuration | string | - | evaluate     
 SPUpdateQueue              | Name of the queue for Update messages | NonSecret | Function Configuration | string | - | update   
 SPDeltaDiscoverySchedule   | Timer schedule for Delta Discovery requests | NonSecret | Function Configuration | string | - | 0 */30 * * * * <br />(See [NCHRON](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-timer?tabs=csharp#ncrontab-expressions))   
 configId                   | Guid for instance of ServicePrincipal Processor, used to find configuration in Config collection in CosmosDB | NonSecret | appSettings.X.json | Guid | - | 02a54ac9-441e-43f1-88ee-fde420db2559   
 aadUpdateMode              | Used to control the behavior of the Update logic | NonSecret | Function Configuration | Enum | Update, ReportOnly | Update   
 visibilityDelayGapSeconds  | Throttle for messages going into Evaluate queue | NonSecret | appSettings.X.json | X | - | 8   
 queueRecordProcessThreshold| Number of queue messages to process in a batch | NonSecret | appSettings.X.json | X | - | 10   
 verboseLogging             | If set to true, all ServicePrincipals retrieved during Discovery will be emitted to the Debug log | NonSecret | Function Configuration | X | true, false | false

