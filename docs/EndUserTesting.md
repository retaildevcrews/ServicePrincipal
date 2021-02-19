# End User Testing

## Prerequisites

- Azure subscription with admin permissions. Any of the following will suffice:
  - Global Administrator
  - Application Administrator
  - Cloud Application Administrator
- Shell Environment (Following Dependencies Should Be Preloaded if Using Cloud Shell)
  - PowerShell ([download](https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell?view=powershell-7.1))
  - Azure CLI ([download](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest))

## Creating Test Service Principals

The Following Service Principals Are Being Created According to the [Test Matrix](#reference-integration-test-matrix)

Modify Service Principals Based On Your Testing Requirements

```sh

# If using Azure Cloud Shell, this step can be skipped
az login

export mySpName=$(az ad signed-in-user show --query "userPrincipalName" -o tsv)

# The Following Script Can Also Remove Owners and Notes with the -RemoveAllOwners and -RemoveNotes Flags
pwsh scripts/create-test-sp.ps1 -SPName "E2E-Testing-TC1"
pwsh scripts/create-test-sp.ps1 -SPName "E2E-Testing-TC2" -SetNotes "blah blah"
pwsh scripts/create-test-sp.ps1 -SPName "E2E-Testing-TC3" -SetNotes joe@gmail.com
pwsh scripts/create-test-sp.ps1 -SPName "E2E-Testing-TC4" -SetNotes joe@walmart.com
pwsh scripts/create-test-sp.ps1 -SPName "E2E-Testing-TC5" -AddOwners $mySpName
pwsh scripts/create-test-sp.ps1 -SPName "E2E-Testing-TC6" -AddOwners $mySpName -SetNotes "blah blah"
pwsh scripts/create-test-sp.ps1 -SPName "E2E-Testing-TC7" -AddOwners $mySpName -SetNotes joe@gmail.com
pwsh scripts/create-test-sp.ps1 -SPName "E2E-Testing-TC8" -AddOwners $mySpName -SetNotes joe@walmart.com

```

## Validate Function Running

```sh

# Input Function App Name, Can be Retrieved from Azure Portal
export FN_NAME="Input the name to your function application"

# Run Following Commands

export RESOURCE_GROUP=$(az functionapp list --query "[?name=='$FN_NAME']" --query "[].resourceGroup" -o tsv)

export FN_MASTER_KEY=$(az functionapp keys list -n $FN_NAME -g $RESOURCE_GROUP --query masterKey -o tsv)

# Validate Following Command Returns JSON
az rest -u "https://$FN_NAME.azurewebsites.net/api/Version?code=$FN_MASTER_KEY" --skip-authorization-header

```

## Processing Service Principals

```sh

# This Will Kick Off A New Delta Discovery Request
# It Is Possible Any Service Principal Changes You Made Were Already Picked Up
export runId=$(az rest -u "https://$FN_NAME.azurewebsites.net/api/RequestDiscovery?code=$FN_MASTER_KEY" --query correlationId -o tsv --skip-authorization-header)

# Re-run This Command Until Status is Completed or Failed
# If This Command Fails, It Is Possible Another Discovery Was Running at the Same Time
az rest -u "https://$FN_NAME.azurewebsites.net/api/Activities?correlationId=$runId&code=$FN_MASTER_KEY" --skip-authorization-header --query "activity[-1].status" -o tsv

```

## Validating Results

```sh

export CosmosEndpoint=$(az functionapp config appsettings list -n $FN_NAME -g $RESOURCE_GROUP --query "[?name=='SPCosmosURL'][].value" -o tsv)

export CosmosDatabase=$(az functionapp config appsettings list -n $FN_NAME -g $RESOURCE_GROUP --query "[?name=='SPCosmosDatabase'][].value" -o tsv)

```

### Accessing Audit Logs

```sh

# Results Sorted by Timestamp Ascending (Latest Results At End)
# Run The Following Command For Each Test Case You Are Testing
# Replace displayName for each Test Case
pwsh ./scripts/query-cosmos.ps1 -CosmosEndpoint $CosmosEndpoint -DatabaseName $CosmosDatabase -CollectionName Audit -Query  "SELECT * FROM c WHERE c.descriptor.displayName = 'E2E-Testing-TC1'"

```

Validate If Audit Passed Or Failed Based On [Test Matrix](#reference-integration-test-matrix)

### Accessing Last Known Good Service Principal

```sh

# Results Sorted by Timestamp Ascending (Latest Results At End)
# Run The Following Command For Each Test Case You Are Testing
# Replace displayName for each Test Case
pwsh ./scripts/query-cosmos.ps1 -CosmosEndpoint $CosmosEndpoint -DatabaseName $CosmosDatabase -CollectionName ObjectTracking -Query  "SELECT * FROM c WHERE c.entity.displayName = 'E2E-Testing-TC1'"

Validate If Last Known Good Was Created Based On [Test Matrix](#reference-integration-test-matrix)

```

## Optional Clean Up

```sh

az login

az ad sp delete --id "E2E-Testing-TC1"
az ad sp delete --id "E2E-Testing-TC2"
az ad sp delete --id "E2E-Testing-TC3"
az ad sp delete --id "E2E-Testing-TC4"
az ad sp delete --id "E2E-Testing-TC5"
az ad sp delete --id "E2E-Testing-TC6"
az ad sp delete --id "E2E-Testing-TC7"
az ad sp delete --id "E2E-Testing-TC8"

```

### Reference Integration Test Matrix

LKG = Last Known Good (Service Principal)

| Test Number | Owner | Notes | Result |
|---|---|---|---|
| 1 | Empty | Empty | Audit Fail, No LKG Created |
| 2 | Empty | Populated But Without Emails | Audit Fail, No LKG Created |
| 3 | Empty | Emails Without Valid Domain Found | Audit Fail, No LKG Created |
| 4 | Empty | Emails With Valid Domain Found | Audit Pass, LKG Created |
| 5 | Non-Empty | Empty | Audit Fail, LKG Created, SP Notes Updated From Owners |
| 6 | Non-Empty | Populated But Without Emails | Audit Fail, LKG Created, SP Notes Updated From Owners |
| 7 | Non-Empty | Emails Without Valid Domain Found | Audit Fail, LKG Created, SP Notes Updated From Owners |
| 8 | Non-Empty | Emails With Valid Domain Found | Audit Fail, LKG Created, No Update to SP Notes (already in a passing state) |
