# Service Principal Policy Automation

![License](https://img.shields.io/badge/license-MIT-green.svg)

## Description

Organizations using AAD often have a challenge with tracking Business Ownership of Service Principals created as part of business solutions deployed in Azure.  While AAD has an `Owners` field as part of it's schema, that field implicitly grants privileges to Users who are added to that list.  Companies need a way to associated non-privileged Business Owners related to the Service Principal to help identify the impacted organization and organizational function in the case that changes must be made that would delete or alter the Service Principal in such a way as to functionally disable the dependent applications.

To help address the need describe above, this solution will check values placed in the Notes field of the Service Principal for valid AAD users and flag those that are non-compliant in tracking data store for reporting and alerting.

## Features

- Seed/re-seed initial tracking data store
- Set default business owners based inital owners list
- Enforce valid structure for data
- Validate users listed in the Notes field to ensure that they are valid AAD users
- Flag invalid entries in the tracking store
- Keep and audit log of all changes detected in the Notes field

## Prerequisites

- Azure subscription with permissions to create:
  - Resource Groups, Service Principals, Key Vault, Cosmos DB, Azure Container Registry, Azure Monitor, App Service, Storage Queues 
- Bash shell (tested on Mac, Ubuntu, Windows with WSL2)
  - Will not work in Cloud Shell unless you have a remote dockerd
- Azure CLI 2.0.72+ ([download](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest))
- Docker CLI ([download](https://docs.docker.com/install/))
- .NET Core SDK 3.1.0 ([download](https://dotnet.microsoft.com/download))
- Azure.Identity 1.2.2
- Azure.Security.KeyVault.Secrets 4.1.0
- Microsoft.Azure.Functions.Extensions 1.0.0
- Microsoft.NET.Sdk.Functions 3.0.9
- Microsoft.Graph 3.12.0
- Microsoft.Graph.Auth 1.0.0-preview.5
- Visual Studio 2019 

## Documentation

- Table of contents is at [docs/index.md](docs/index.md)

## Contributing

This project welcomes contributions and suggestions. Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit [Microsoft Contributor License Agreement](https://cla.opensource.microsoft.com).

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
