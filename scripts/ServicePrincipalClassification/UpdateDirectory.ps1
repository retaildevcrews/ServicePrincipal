<#
    .SYNOPSIS
    Updates ServicePrincipals in an inputfile through the Graph API.
#>

[CmdletBinding(SupportsShouldProcess=$true)]
param (
  [Parameter(Mandatory=$true)]
  [string]
  # Path of the file for Input
  $InputFile = $null,

  [Parameter(Mandatory=$true)]
  [ValidateSet("csv", "tsv", "json")]
  [string]
  # Format for the output: csv, tsv, json
  $InputType = "csv"
)


function ValidateArguments
{
    Param ()
}

# Validate arguments
ValidateArguments

Write-Host "`tLoading Input from $InputFile" -ForegroundColor Green
if ($InputType -eq "csv") 
{
    $spList = Get-Content $InputFile | ConvertFrom-Csv
} 
elseif ($InputType -eq "tsv") 
{
    $spList = Get-Content $InputFile | ConvertFrom-Tsv
} 
elseif ($InputType -eq "json") 
{
    $spList = Get-Content $InputFile | ConvertFrom-Json
} 

Write-Host "`t`t$($spList.Count) ServicePrincipals loaded from $InputFile" -ForegroundColor Green
$updateList = $spList | ? { [string]::IsNullOrWhitespace($_.Notes) -and -not([string]::IsNullOrWhiteSpace($_.AppOwners)) }

Write-Host "`t$($updateList.Count) ServicePrincipals found to update"

if ($updateList.Count -gt 0)
{
    # Connect to the Graph API and get all the service principals
    Write-Host "`tConnecting to Graph" -ForegroundColor Green
    connect-graph -Scopes "Application.ReadWrite.All" | Out-Null

    Write-Host "`tUpdating ServicePrincipals" -ForegroundColor Yellow
    foreach ($sp in $updateList)
    {
        $target = "$($sp.DisplayName) ($($sp.Id)) : [$($sp.AppOwners)]"
        if ($PSCmdlet.ShouldProcess($target,"Update Notes from AppOwners"))
        {
            Write-Host "`tUpdate $target"
            Update-MgServicePrincipal -ServicePrincipalId $sp.Id -Notes $sp.AppOwners
        }
    
    }

    Write-Host "`tDisconnecting from graph" -ForegroundColor Green
    #disconnect-graph     
}

Write-Host -ForegroundColor Green "Done."