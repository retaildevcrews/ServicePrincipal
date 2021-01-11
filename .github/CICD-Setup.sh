#!/bin/bash

#Admin Check
echo "Checking if User is Administrator"

userPrincipalName=$(az ad signed-in-user show --query "userPrincipalName" -o tsv)
AdminTemplateRoles=$(az rest --url https://graph.windows.net/myorganization/roleDefinitions?api-version=1.61-internal --query "value[?displayName=='Company Administrator' || displayName=='Global Administrator' || displayName=='Application Administrator' || displayName=='Cloud Application Administrator'].templateId" -o tsv)
adminRoleDetected=false
for role in $AdminTemplateRoles
do
  RoleUsers=$(az rest --url "https://graph.windows.net/myorganization/roleAssignments?\$filter=roleDefinitionId eq '$role'&\$expand=principal&api-version=1.61-internal" --query "value[].principal[].userPrincipalName" -o tsv)
  for user in $RoleUsers
  do
    if [ "$user" == "$userPrincipalName" ]; then
      adminRoleDetected=true
    fi
  done
done

if [ "$adminRoleDetected" == true ]; then
    echo "Validated User is Admin"
else
    >&2 echo "WARNING: Admin Permission Not Validated"
fi

#Prompt User For Name of App Registration To Create
read -p "Enter Name of ServicePrincipal To Create To Serve As Connection For GitHub Actions: " SP_NAME

# Validate App Registration Doesn't Already Exist
AppId=$(az ad sp show --id "http://$SP_NAME" --query appId -o tsv 2> /dev/null)
if [ ! -n "$AppId" ]; then
  echo "ServicePrincipal Doesn't Exist. Will Create"
else
  >&2 echo "ServicePrincipal Already Exists. Please Remove First"
  >&2 echo "run: az ad app delete --id $AppId"
  exit 1
fi

# Prompt User for Github Information
read -p "Enter GitHub Username: " GH_USER
read -p "Enter GitHub Personal Token With Repo Access: " GH_TOKEN
read -p "Enter GitHub Org That Contains New Repository: " GH_ORG
read -p "Enter GitHub Repository Name: " GH_REPO

# Validate Github Prompts
TempKey=$(curl -u $GH_USER:$GH_TOKEN https://api.github.com/repos/$GH_ORG/$GH_REPO/actions/secrets/public-key | jq -r '.key')
if [ $TempKey == null ]; then
  echo "Unable to Login To Repository"
else
  >&2 echo "Able to Login To Repository"
  exit 1
fi

# Create Service Principal with RBAC
export SERVICE_PRINCIPAL_SECRET=$(az ad sp create-for-rbac -n $SP_NAME --query password -o tsv)

export SERVICE_PRINCIPAL_ID=$(az ad sp show --id "http://$SP_NAME" --query appId -o tsv)

export TENANT_ID=$(az account show -o tsv --query tenantId)

export GRAPH_ID=$(az ad sp list --query "[?appDisplayName=='Microsoft Graph'].appId | [0]" --all -o tsv)

export appReadWriteAll=$(az ad sp show --id $GRAPH_ID --query "oauth2Permissions[?value=='Application.ReadWrite.All'].id | [0]" -o tsv)

# Add App persmission
az ad app permission add --id $SERVICE_PRINCIPAL_ID --api $GRAPH_ID --api-permissions $appReadWriteAll=Scope

# Make permissions effective
az ad app permission grant --id $SERVICE_PRINCIPAL_ID --api $GRAPH_ID

# Admin consent
az ad app permission admin-consent --id $SERVICE_PRINCIPAL_ID

# Push Secrets To Github
pwsh .github/put-github-secret.ps1 -UserName $GH_USER -PersonalToken $GH_TOKEN -OrgAndRepo "$GH_ORG/$GH_REPO" -SecretKey SERVICE_PRINCIPAL_SECRET -SecretVal "$SERVICE_PRINCIPAL_SECRET"

pwsh .github/put-github-secret.ps1 -UserName $GH_USER -PersonalToken $GH_TOKEN -OrgAndRepo "$GH_ORG/$GH_REPO" -SecretKey SERVICE_PRINCIPAL_ID -SecretVal "$SERVICE_PRINCIPAL_ID"

pwsh .github/put-github-secret.ps1 -UserName $GH_USER -PersonalToken $GH_TOKEN -OrgAndRepo "$GH_ORG/$GH_REPO" -SecretKey TENANT_ID -SecretVal "$TENANT_ID"
