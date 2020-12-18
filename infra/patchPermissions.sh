appServicePrincipalId=$1
echo "appServicePrincipalId: $appServicePrincipalId"


# Get MSGraphId
export graphId=$(az ad sp list --query "[?appDisplayName=='Microsoft Graph'].appId | [0]" --all) 
graphId=$(eval echo $graphId)
echo "Service MSGraph AppID: " $graphId
  
export appRoleAppReadWriteAll=$(az ad sp show --id $graphId --query "appRoles[?value=='Application.ReadWrite.All'].id | [0]") 
appRoleAppReadWriteAll=$(eval echo $appRoleAppReadWriteAll)
echo "Application- Application.ReadWrite.All ID: " $appRoleAppReadWriteAll

export appRoleDirReadAll=$(az ad sp show --id $graphId --query "appRoles[?value=='Directory.Read.All'].id | [0]") 
appRoleDirReadAll=$(eval echo $appRoleDirReadAll)
echo "application- Directory.Read.All id:" $appRoleDirReadAll

# Add App permission 
az ad app permission add --id $appServicePrincipalId --api $graphId --api-permissions $appRoleDirReadAll=Role $appRoleAppReadWriteAll=Role

# Make permissions effective
az ad app permission grant --id $appServicePrincipalId --api $graphId

# Admin consent
az ad app permission admin-consent --id $appServicePrincipalId


