[CmdletBinding()]
param (
  [string]$OutputFile = $null,
  [switch]$CSV
)

$GroupMetadata = Get-Content ./groups.json | ConvertFrom-Json

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

  
  $classification = $GroupMetadata | % {
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
  $spList | Group-Object {$_.Tags[-2..-1] -join ", "} | Sort-Object Count -D

  # Classification, Category, SP Id, OwningOrgId, AppId, AppName, ServicePrincipalNames, serviceprincipaltype, owning domain name
  if ([string]::IsNullOrWhiteSpace($OutputFile)) {
    if ($CSV) {
      # $outputstuff  | Export-Csv $OutputFile
    }
    else {
      # $outputstuff  | Out-File $OutputFile
    }
  }
  else {
    # Write-Output $outputstuff
  }