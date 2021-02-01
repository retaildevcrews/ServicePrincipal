[CmdletBinding()]
param (
  [string]$SPName,

  [switch]$RemoveAllOwners,

  [string[]]$RemoveOwners,

  [string[]]$AddOwners,

  [switch]$RemoveNotes,

  [string]$SetNotes
)

# Create App Registration and Service Principal
if ([string]::IsNullOrWhiteSpace($SPName)) {
  Write-Error "Service Principal Name Must Be Set. Example Argument: -SPName TestSP"
} else {
  az ad sp create-for-rbac -n "http://$SPName" --skip-assignment | out-null
}

# Wait For Service Principal To Propagate
while ([string]::IsNullOrWhiteSpace($spId)){
  $spId = az ad sp show --id "http://$SPName" --query objectId -o tsv
  Start-Sleep -s 1
}

function GetSpId
{
  param (
    [string]$owner
  )

  try {
    [guid]::Parse($owner)
    return $owner
  }
  catch
  {
    $id = az ad user list --upn $owner --query "[].objectId" -o tsv
    if ($null -ne $id) {
      return $id
    } else {
      Write-Error "Could not find owner: $owner"
      exit 1
    }

  }
}

function RemoveOwner
{
  param (
    [string]$spId,
    [string]$owner
  )
  $ownerId = GetSpId $owner
  az rest -m delete -u "https://graph.microsoft.com/v1.0/serviceprincipals/$spId/owners/$ownerId/`$ref" -b "{\`"@odata.id\`":\`"https://graph.microsoft.com/v1.0/directoryObjects/$ownerId\`"}"
}

function AddOwner
{
  param (
    [string]$spId,
    [string]$owner
  )
  $ownerId = GetSpId $owner
  az rest -m post -u "https://graph.microsoft.com/v1.0/serviceprincipals/$spId/owners/$ownerId/`$ref" -b "{\`"@odata.id\`":\`"https://graph.microsoft.com/v1.0/directoryObjects/$ownerId\`"}"
}

if ($RemoveAllOwners) {
  $idsToRemove = az rest -u "https://graph.microsoft.com/v1.0/serviceprincipals/$spId/owners" --query "value[].id" -o tsv
  $idsToRemove | ForEach-Object { RemoveOwner $spId $_ }
}

# Remove User Specified Owners
$RemoveOwners | ForEach-Object { RemoveOwner $spId $_ }

# Add User Specified Owners
$AddOwners | ForEach-Object { AddOwner $spId $_ }

# Remove Notes
if ($RemoveNotes)
{
  az rest -m patch -u "https://graph.microsoft.com/v1.0/serviceprincipals/$spId" -b "{\`"notes\`":null}"
}

# Update Notes
if ($null -ne $SetNotes)
{
  az rest -m patch -u "https://graph.microsoft.com/v1.0/serviceprincipals/$spId" -b (@{notes=$SetNotes} | ConvertTo-Json)
}
Write-Host "Successfully Updated $SPName"
Write-Host "Following Owners Are Set:"
az rest -u "https://graph.microsoft.com/v1.0/serviceprincipals/$spId/owners" --query "value[].userPrincipalName" | Write-Host

Write-Host "Following Notes Are Set:"
az rest -u "https://graph.microsoft.com/v1.0/serviceprincipals/$spId" --query "notes"
