[CmdletBinding()]
param (
  [string]$OutputFile = $null,
  [switch]$CSV
)

$ClassificationMapping = Get-Content './resoureces/classification_mapping.json' | ConvertFrom-Json

$CategoryOobeList = Get-Content './resources/category_oobe_list.json' | ConvertFrom-Json

function ClassifyMicrosoft
{
  [CmdletBinding()]
  param (
    $group
  )
  $group.Group |
    ForEach-Object {
      $_.Tags += "Microsoft"
      if ($CategoryOobeList -contains $_.AppId){
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

  
  $classification = $ClassificationMapping | % {
    if ($_.MatchValues -contains $groupName)
    {
      return $_.Classification
    }
  }

  if ([string]::IsNullOrWhiteSpace($classification)) {
    return "ThirdParty"
  }
  else {
    return $classification
  }
}

connect-graph -Scopes "Directory.read.all"
$spList = Get-MgServicePrincipal -All

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

  if ([string]::IsNullOrWhiteSpace($OutputFile)) {
    $results = $spList |
      Select-Object -Property @{N='Classification';E={$_.Tags[-2]}}, @{N='Category';E={$_.Tags[-1]}}, Id, AppOwnerOrganizationId, AppId, DisplayName, ServicePrincipalNames, ServicePrincipalType
    if ($CSV) {
      $results | Export-Csv -Path $OutputFile -NoTypeInformation
    }
    else {
      $results | ConvertFrom-Json | Out-File -FilePath $OutputFile
    }
  }
  else {
    Write-Output $results
  }
