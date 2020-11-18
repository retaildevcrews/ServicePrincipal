connect-graph -Scopes "Directory.read.all"
$spList = Get-MgServicePrincipal -All

function ClassifyMicrosoft
{
  [CmdletBinding()]
  param (
    $group
  )
  $oobeList = Get-Content './resources/microsoft_oobe_mapping.json' | ConvertFrom-Json
  $group.Group |
    ForEach-Object {
      $_.Tags += "Microsoft"
      if ($oobeList -contains $_.AppId){
        $_.Tags += "OOBE"
      }
      else {
        $_.Tags += "Addition"
      }
    }
}

function ClassifyTenant
{
  [CmdletBinding()]
  param (
    $group
  )
  $group.Group |
    ForEach-Object {
      $_.Tags += "Tenant"
      $_.Tags += "None"
    }
}

function ClassifyThirdParty
{
  [CmdletBinding()]
  param (
    $group
  )
  $group.Group |
    ForEach-Object {
      $_.Tags += "ThirdParty"
      $_.Tags += "None"
    }
}

function ClassifyGroup
{
  [CmdletBinding()]
  param (
    [string]$groupName
  )

  if ("Application, f8cdef31-a31e-4b4a-93e4-5f571e91255a", "Application, 72f988bf-86f1-41af-91ab-2d7cd011db47", "Application", "Legacy", "SocialIdp" -contains $groupName) {
    return "Microsoft"
  }
  elseif ("Application, 9c26242b-1b00-450c-98b0-a31412ad5a0e", "ManagedIdentity" -contains $groupName) {
    return "Tenant"
  }
  else {
    return "ThirdParty"
  }
}

$groups = $spList | Group-Object -Property ServicePrincipalType,AppOwnerOrganizationId
$groups | 
  ForEach-Object {
    $groupClass = ClassifyGroup $_.Name
    if ($groupClass -eq 'Microsoft') {
      ClassifyMicrosoft $_
    }
    elseif ($groupClass -eq 'Tenant') {
      ClassifyTenant $_
    }
    else {
      ClassifyThirdParty $_
    }
  }
  $spList | Group-Object {$_.Tags[-2..-1] -join ", "} | Sort-Object Count -D
