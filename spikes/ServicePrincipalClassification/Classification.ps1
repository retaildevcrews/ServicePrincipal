[CmdletBinding()]
param (
  [string]$OutputFile = $null,
  [string]$OutputType = "csv"
)

function ValidateArguments
{
  $SupportedTypes = @("csv", "tsv", "json")
  if ([string]::IsNullOrWhiteSpace($OutputFile)) {
    Write-Host "File Path Not Set, Only Printing Summary"
  }
  if ($SupportedTypes -contains $OutputType.ToLower()) {
    $OutputType = $OutputType.ToLower()
    Write-Host "Output Type Set To '$OutputType'"
  } else {
    Write-Error "Only Output Types Supported are: $SupportedTypes"
  }
}

$ClassificationMapping = Get-Content './resources/classification_mapping.json' | ConvertFrom-Json

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
      $_.Tags += $_.ServicePrincipalType
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
      $_.Tags += $_.ServicePrincipalType
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

ValidateArguments

connect-graph -Scopes "Directory.read.all"
$spList = Get-MgServicePrincipal -All

$groups = $spList | Group-Object -Property ServicePrincipalType,AppOwnerOrganizationId | Sort-Object Count -D

Write-Host "`nSummary of Service Principals Retrieved:"
$groups | Select -Property Count, @{N='AppOwnerOrganizationId';E={$_.Group[0].AppOwnerOrganizationId}}, @{N='ServicePrincipalType';E={$_.Group[0].ServicePrincipalType}} | Out-String | Write-Host

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

  if (-not ([string]::IsNullOrWhiteSpace($OutputFile))) {
    $results = $spList |
      Select-Object -Property @{N='Classification';E={$_.Tags[-2]}}, @{N='Category';E={$_.Tags[-1]}}, Id, AppOwnerOrganizationId, AppId, DisplayName, @{N='ServicePrincipalNames';E={$_.ServicePrincipalNames -join ", "}}, ServicePrincipalType
    
    Write-Host "Summary of Results:"
    $results | Group-Object -Property Classification, Category | Sort-Object Count -D | Select -Property Count, @{N='Classification';E={$_.Group[0].Classification}}, @{N='Category';E={$_.Group[0].Category}} | Out-String | Write-Host

    if ($OutputType -eq "csv") {
      $results | ConvertTo-Csv | Out-File -FilePath $OutputFile
    } elseif ($OutputType -eq "tsv") {
      $results | ConvertTo-Csv -Delimiter "`t" | Out-File -FilePath $OutputFile
    } elseif ($OutputType -eq "json") {
      $results | ConvertTo-Json | Out-File -FilePath $OutputFile
    } else {
      Write-Error "not supported type: $OutputType"
    }
  }
