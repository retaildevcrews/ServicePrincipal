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

## Clone Repository

Create a new repository in your Github Organization

- [ ] Copy URI of new repository to use in next step

```sh

git clone git@github.com:retaildevcrews/ServicePrincipal.git
cd ServicePrincipal

# Please Note SSH and HTTPS Connection URIs Vary
git remote set-url origin git@github.com:retaildevcrews/ServicePrincipal.git
git remote set-url origin < ENTER URI TO NEW REPO HERE >

git branch -M main

# If Following Push Fails, Set Email and Name and Try Again
git push -u origin main

```

## Create Github SP Connector and Push Secrets To Repo

### Create Personal Token

Follow steps in this [guide](https://docs.github.com/en/free-pro-team@latest/github/authenticating-to-github/creating-a-personal-access-token) to create a personal access token. This will be used to create and push sensitive tokens used by CICD

For Scopes / Permissions, check "repo" Box

- [ ] Copy token for later prompting

After completing this walkthrough this token can be removed using GitHub UI

### Capture Additional Repo Information

Capture the following information and save for later prompting:

- [ ] Name of ServicePrincipal To Create To Serve As Connection For GitHub Actions. Example: GitHubConnector
- [ ] Github Username
- [ ] Your Github Organization That Contains Repository
- [ ] Your Github Repository Name

### Run Script to Create SP Connection and Push Secrets

```sh

az login

chmod +x .github/CICD-Setup.sh

#Follow Enter Previously Captured Information When Script Prompts
.github/CICD-Setup.sh

```

## First GitHub Action Run

```sh

code .github/workflows/dockerCI.yml

# Replace Value of jobs.build.env.RESOURCE_GROUP with Resource Group Created By Infrastructure Scripts (Can Also Be Retreived From portal.azure.com)

# Save File

# Stage and Commit
git add .
git commit -m "First Push"

# If Following Push Fails, Set Email and Name and Try Again
git push

```

Navigate to your Github Repository and Click on Actions Tab

You should see a new workflow was kicked off after you pushed your changes
