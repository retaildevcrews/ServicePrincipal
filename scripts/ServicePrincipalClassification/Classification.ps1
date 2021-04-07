<#
    .SYNOPSIS
    Pulls ServicePrincipals from AAD through the Graph API and classifies them.
#>

[CmdletBinding()]
param (
  [string]
  # Path of the file for output
  $OutputFile = $null,

  [ValidateSet("csv", "tsv", "json")]
  [string]
  # Format for the output: csv, tsv, json
  $OutputType = "csv", 

  [int]
  # Limit results to the first N 
  $Top = $null,

  [switch]
  # Switch to perform AppOwner enrichment from Application Object
  $Enrich, 

  [switch]
  # Switch to pass output through pipeline
  $PassThru
)


function ValidateArguments
{
  if ([string]::IsNullOrWhiteSpace($OutputFile) -and -not $PassThru) {
    Write-Host -ForegroundColor Yellow "`nOutput file path not set, only printing summary`n"
  }
}

function Classify
{
  param(
    [object]$obj,
    [string]$Classification,
    [string]$Category,
    [string]$OwningDomain
  )

  $obj | Add-Member -NotePropertyName Classification -NotePropertyValue $Classification
  $obj | Add-Member -NotePropertyName Category -NotePropertyValue $Category
  $obj | Add-Member -NotePropertyName OwningDomain -NotePropertyValue $OwningDomain
}

function ClassifyMicrosoft
{
  [CmdletBinding()]
  param (
    $group
  )
  $group.Group |
    ForEach-Object {
      
      if ($CategoryOobeList -contains $_.AppId)
      {
        $category = "OOBE"
      }
      else 
      {
        $category = "Addition"
      }

      Classify $_ "Microsoft" $category ""
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
      Classify $_ "Tenant" $_.ServicePrincipalType ""
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
      Classify $_ "ThirdParty" $_.ServicePrincipalType (($_.ServicePrincipalNames.split(", ") | ? { -not($_.ToLower().Contains("*"))  } | % {[System.Uri]$_} | ? {$null -ne $_.Host} | % {$_.Host} | Get-Unique ) -join ", ")
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

function EnrichAppOwners
{
  param ($sp)
  #$appId = $sp.AppId
  #$app = Get-MgApplication -Filter "appId eq '$appId'" -ExpandProperty Owners
  $app = $appList | ? { $_.AppId -eq $sp.AppId }
  if ($null -ne $app)
  {
    $sp.AppOwners = (($app.Owners | % { 
        $UPNs = ""
        if ($_.AdditionalProperties['@odata.type'] -eq "#microsoft.graph.servicePrincipal") {
          $UPNs = ((Get-MgServicePrincipal -ServicePrincipalId $_.Id -ExpandProperty Owners).Owners | % { $_.AdditionalProperties['userPrincipalName'] }) -join ','
        }
        elseif ($_.AdditionalProperties['@odata.type'] -eq "#microsoft.graph.user")
        {
          $UPNs = $_.AdditionalProperties['userPrincipalName']
        }
        $UPNs
      }) -join ',')
  }

}

# Validate arguments
ValidateArguments

# Load mappings
Write-Host -ForegroundColor Green "`tLoading maps"
$ClassificationMapping = Get-Content './resources/classification_mapping.json' | ConvertFrom-Json
$CategoryOobeList = Get-Content './resources/category_oobe_list.json' | ConvertFrom-Json


# Connect to the Graph API and get all the service principals
Write-Host -ForegroundColor Green "`tConnecting to Graph"
connect-graph -Scopes "Directory.read.all" | Out-Null

Write-Host -ForegroundColor Green "`tQuerying Applications"
$appList = Get-MgApplication -ExpandProperty Owners -All
Write-Host "`t`t$($appList.Count) Applications retrieved"

Write-Host -ForegroundColor Green "`tQuerying ServicePrincipals"

if ($null -ne $Top)
{
  $spList = Get-MgServicePrincipal -All -ExpandProperty Owners | select -first $Top
} 
else 
{
  $spList = Get-MgServicePrincipal -All -ExpandProperty Owners
}
Write-Host "`t`t$($spList.Count) ServicePrincipals retrieved"

# Group the list by type and owner org
Write-Host -ForegroundColor Green "`tSorting and Grouping"
$groups = $spList | Group-Object -Property ServicePrincipalType,AppOwnerOrganizationId | Sort-Object Count -D

Write-Host -ForegroundColor Green "`tClassifying"
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

  $spList | Add-member -NotePropertyName AppOwners -NotePropertyValue ""
  if ($Enrich)
  {
    $filteredList = $spList | ? { ($_.ServicePrincipalType -eq "Application") -and ($_.Classification -eq "Tenant") }
    Write-Host -ForegroundColor Green "`tEnriching AppOwners ($($filteredList.Count))"

    # this is a long process and we want to see some status, use a for loop instead of a pipe
    for ($i = 0; $i -lt $filteredList.Count; $i++) {
      $item = $filteredList[$i]
      EnrichAppOwners $item

      if (($i -gt 0) -and ($i % 200) -eq 0)
      {
        Write-Host "$($i)/$($filteredList.Length).." -NoNewline
      }
    }
    Write-Host ""
    #$filteredList | % { EnrichAppOwners $_ }
  }
  

  Write-Host "`nServicePrincipals by AppOwnerOrganizationId, Type:"
  $groups | Select-Object -Property Count, @{N='AppOwnerOrganizationId';E={$_.Group[0].AppOwnerOrganizationId}}, @{N='ServicePrincipalType';E={$_.Group[0].ServicePrincipalType}} | Out-String | Write-Host


  $results = $spList |
      Select-Object -Property Classification, Category, OwningDomain, Id, AppOwnerOrganizationId, DisplayName, AppId, Notes, AppOwners, @{N='ServicePrincipalNames';E={$_.ServicePrincipalNames -join ", "}}, ServicePrincipalType

  Write-Host "Summary of Classified ServicePrincipals:"
  $results | Group-Object -Property Classification, Category | Sort-Object Count -D | Select-Object -Property Count, @{N='Classification';E={$_.Group[0].Classification}}, @{N='Category';E={$_.Group[0].Category}} | Out-String | Write-Host

    # Emit output file if we have a filename
  if (-not ([string]::IsNullOrWhiteSpace($OutputFile))) 
  {
    Write-Host -ForegroundColor Green "`tEmitting $OutputType file"

    if ($OutputType -eq "csv") {
      $results | ConvertTo-Csv | Out-File -FilePath $OutputFile
    } elseif ($OutputType -eq "tsv") {
      $results | ConvertTo-Csv -Delimiter "`t" | Out-File -FilePath $OutputFile
    } elseif ($OutputType -eq "json") {
      $results | ConvertTo-Json | Out-File -FilePath $OutputFile
    } 
  }

  Write-Host -ForegroundColor Green "`tDisconnecting from graph"
  disconnect-graph 

  if ($PassThru) {
    return $results
  }



  Write-Host -ForegroundColor Green "Done."