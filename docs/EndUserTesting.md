# End User Testing

## Prerequisites

Azure Active Directory Permissions To Create Service Principals
Infrastructure Already Setup
CICD Workflow has run at least once Successfully
Bash
Powershell

## Validate Service Is Operational

## Creating Test Service Principals

```sh

# If using Azure Cloud Shell, this step can be skipped
az login

export userInitials=$(az ad signed-in-user show --query "displayName" -o tsv | sed 's/\(\w\)\w*/\1/g' | sed 's/\s//g')

pwsh infra/cicd/create-test-sp.ps1 -SPName "$userInitials-No-Owner-No-Notes" -Owner none
pwsh infra/cicd/create-test-sp.ps1 -SPName "$userInitials-No-Owner-No-Email" -Owner none -Notes "blah blah"
pwsh infra/cicd/create-test-sp.ps1 -SPName "$userInitials-No-Owner-Invalid-Email" -Owner none -Notes joe@gmail.com
pwsh infra/cicd/create-test-sp.ps1 -SPName "$userInitials-No-Owner-Valid-Email" -Owner none -Notes joe@walmart.com
pwsh infra/cicd/create-test-sp.ps1 -SPName "$userInitials-Owner-Set-No-Notes" -Owner myself
pwsh infra/cicd/create-test-sp.ps1 -SPName "$userInitials-Owner-Set-No-Email" -Owner myself -Notes "blah blah"
pwsh infra/cicd/create-test-sp.ps1 -SPName "$userInitials-Owner-Set-Invalid-Email" -Owner myself -Notes joe@gmail.com
pwsh infra/cicd/create-test-sp.ps1 -SPName "$userInitials-Owner-Set-Valid-Email" -Owner myself -Notes joe@walmart.com

```

NOPE - One Time Script

Manual Creation

## Processing Service Principals

NOPE - Run Integration Tests

Navigate To Function App

Requst Manual Delta Run

Wait For Completion

## Validating Results

### Accessing Audit Logs

Navigate To Cosmos DB

## Optional Clean Up

```sh

az login

az ad sp delete --id "http://$userInitials-No-Owner-No-Notes"
az ad sp delete --id "http://$userInitials-No-Owner-No-Email"
az ad sp delete --id "http://$userInitials-No-Owner-Invalid-Email"
az ad sp delete --id "http://$userInitials-No-Owner-Valid-Email"
az ad sp delete --id "http://$userInitials-Owner-Set-No-Notes"
az ad sp delete --id "http://$userInitials-Owner-Set-No-Email"
az ad sp delete --id "http://$userInitials-Owner-Set-Invalid-Email"
az ad sp delete --id "http://$userInitials-Owner-Set-Valid-Email"

```

### Integration Test Matrix

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
