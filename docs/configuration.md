![License](https://img.shields.io/badge/license-MIT-green.svg)
<h1>Application Configuration</h1>
This document discusses the configuration strategy and configuration settings for each environment type.  
    
- [Overview](#overview)
- [Environments](#environments)
- [Configuration Sources](#configuration-sources)
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
 local.settings.json | NonSecret | Local, Non-Production, Production
 appsettings.Development.json | NonSecret | Local, Non-Production
 appsettings.Production.json | NonSecret | Production
 KeyVault | Secret | Non-Production, Production

## Local Development Environment
## Shared Environment
### Shared Development Environment
### Shared QA Environment
## Production Environment
