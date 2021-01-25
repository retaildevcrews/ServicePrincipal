[CmdletBinding()]
param (
  [string]$SPName,

  [ValidateSet("none", "myself")]
  [string]$Owner,

  [string]$Notes
)
$userObjectId = az ad signed-in-user show --query "objectId" -o tsv

# Create App Registration and Service Principal
az ad sp create-for-rbac -n "http://$SPName" --skip-assignment #| out-null

#Start-Sleep -s 2

while ([string]::IsNullOrWhiteSpace($spId)){
  $spId = az ad sp show --id "http://$SPName" --query objectId -o tsv
  Start-Sleep -s 1
}

#az rest -m delete -u "https://graph.microsoft.com/v1.0/serviceprincipals/$spId/owners/$userObjectId/`$ref" -b "{\`"@odata.id\`":\`"https://graph.microsoft.com/v1.0/directoryObjects/$userObjectId\`"}"
if ($Owner -eq 'myself')
{
  az rest -m post -u "https://graph.microsoft.com/v1.0/serviceprincipals/$spId/owners/`$ref" -b "{\`"@odata.id\`":\`"https://graph.microsoft.com/v1.0/directoryObjects/$userObjectId\`"}"
}

if (-not ([string]::IsNullOrWhiteSpace($Notes)))
{
  az rest -m patch -u "https://graph.microsoft.com/v1.0/serviceprincipals/$spId" -b "{\`"notes\`":\`"$Notes\`"}"
}
else
{
  az rest -m patch -u "https://graph.microsoft.com/v1.0/serviceprincipals/$spId" -b "{\`"notes\`":null}"
}

Write-Host "Following Owners Are Set:"
az rest -u "https://graph.microsoft.com/v1.0/serviceprincipals/$spId/owners" | Write-Host
Write-Host "Following Notes Are Set: $(az rest -u "https://graph.microsoft.com/v1.0/serviceprincipals/$spId" --query "notes")"
