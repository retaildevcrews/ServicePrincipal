# First Time CI / CD Setup

## Prerequisites

- Azure subscription with admin permissions. Any of the following will suffice:
  - Global Administrator
  - Application Administrator
  - Cloud Application Administrator
- Bash Environment (Following Dependencies Should Be Preloaded if Using Cloud Shell)
  - PowerShell ([download](https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell?view=powershell-7.1))
  - Azure CLI ([download](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest))
- Infrastructure Setup: [infra/README.md](../infra/README.md)

## Fork Repository

Navigate to [retaildevcrews/ServicePrincipal](https://github.com/retaildevcrews/ServicePrincipal)

In the top-right corner of the page, click Fork.

![How to fork a repository](images/fork-repo.png)

Place in destination organization

- [ ] Copy URI of new repository to use in next step

## Clone Repository To Local

```sh

# Please Note SSH and HTTPS Connection URIs Differ
git clone < ENTER URI TO NEW REPO HERE >
cd ServicePrincipal

```

## Create Personal Token

Follow steps in this [guide](https://docs.github.com/en/free-pro-team@latest/github/authenticating-to-github/creating-a-personal-access-token) to create a personal access token. This will be used to create and push sensitive tokens used by CICD

For Scopes / Permissions, check "repo" Box

- [ ] Copy token for later prompting

After completing this walkthrough this token can be removed using GitHub UI

## Capture Additional Repo Information

Capture the following information and save for later prompting:

- [ ] Name of ServicePrincipal To Create To Serve As Connection For GitHub Actions. Example: GitHubConnector
- [ ] Github Username
- [ ] Your Github Organization That Contains Repository
- [ ] Your Github Repository Name

## Run Script to Create SP Connection and Push Secrets

```sh

az login

chmod +x infra/cicd/cicd-setup.sh

#Follow Enter Previously Captured Information When Script Prompts
infra/cicd/cicd-setup.sh

```

## Create Service Principals For Integration Testing

```sh

export RESOURCE_GROUP=< ENTER RESOURCE GROUP CREATED HERE >
export KEYVAULT_NAME=$(az resource list -g ${RESOURCE_GROUP} --resource-type Microsoft.KeyVault/vaults --query '[].name' -o tsv)
export AUTH_TYPE=CLI
dotnet run --project src/Automation/CSE.Automation.TestsPrep/CSE.Automation.TestsPrep.csproj

```

## First GitHub Action Run

```sh

sed -i "s/\(\s*RESOURCE_GROUP: \)\(.*\)/\1$RESOURCE_GROUP/g" .github/workflows/dockerCI.yml

# Stage and Commit
git add .
git commit -m "First Push"

# If Following Push Fails, Set Email and Name and Try Again
git push

```

Navigate to your Github Repository and Click on Actions Tab

You should see a new workflow was kicked off after you pushed your changes

## Getting Upstream Updates

You may periodically want to pull updates from the original repository.

To do that you can sync your fork with the upstream repository.

[GitHub Docs - Syncing a Fork](https://docs.github.com/en/free-pro-team@latest/github/collaborating-with-issues-and-pull-requests/syncing-a-fork)
