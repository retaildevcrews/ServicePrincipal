[CmdletBinding()]
param (
  [string]$OutputFile = $null,

  [ValidateSet("csv", "tsv", "json")]
  [string]$OutputType = "csv", 

  [switch]$PassThru
)

function ValidateArguments
{
  if ([string]::IsNullOrWhiteSpace($OutputFile) -and -not $PassThru) {
    Write-Host "File Path Not Set, Only Printing Summary"
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
      $_.Tags += ""
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
      $_.Tags += ""
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
      $_.Tags += (($_.ServicePrincipalNames.split(", ") |
                   ForEach-Object {[System.Uri]$_} |
                   Where-Object {$null -ne $_.Host} |
                   ForEach-Object {$_.Host} |
                   Get-Unique ) -join ", ")
    }
}

function ClassifyGroup
{
  [CmdletBinding()]
  param (
    [string]$groupName
  )

  
  $classification = $ClassificationMapping | ForEach-Object {
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

connect-graph -Scopes "Directory.read.all" | Out-Null
$spList = Get-MgServicePrincipal -All

$groups = $spList | Group-Object -Property ServicePrincipalType,AppOwnerOrganizationId | Sort-Object Count -D

Write-Host "`nSummary of Service Principals Retrieved:"
$groups | Select-Object -Property Count, @{N='AppOwnerOrganizationId';E={$_.Group[0].AppOwnerOrganizationId}}, @{N='ServicePrincipalType';E={$_.Group[0].ServicePrincipalType}} | Out-String | Write-Host

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

  $results = $spList |
      Select-Object -Property @{N='Classification';E={$_.Tags[-3]}}, @{N='Category';E={$_.Tags[-2]}}, @{N='OwningDomain';E={$_.Tags[-1]}}, Id, AppOwnerOrganizationId, AppId, DisplayName, @{N='ServicePrincipalNames';E={$_.ServicePrincipalNames -join ", "}}, ServicePrincipalType

  Write-Host "Summary of Results:"
      $results | Group-Object -Property Classification, Category | Sort-Object Count -D | Select-Object -Property Count, @{N='Classification';E={$_.Group[0].Classification}}, @{N='Category';E={$_.Group[0].Category}} | Out-String | Write-Host

  if (-not ([string]::IsNullOrWhiteSpace($OutputFile))) {

    if ($OutputType -eq "csv") {
      $results | ConvertTo-Csv | Out-File -FilePath $OutputFile
    } elseif ($OutputType -eq "tsv") {
      $results | ConvertTo-Csv -Delimiter "`t" | Out-File -FilePath $OutputFile
    } elseif ($OutputType -eq "json") {
      $results | ConvertTo-Json | Out-File -FilePath $OutputFile
    } 

  }
  else {
    return $results
  }
