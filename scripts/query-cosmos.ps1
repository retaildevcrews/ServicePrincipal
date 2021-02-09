[CmdletBinding()]
param (
  [Parameter(Mandatory=$true)][String]$CosmosEndpoint,
  [Parameter(Mandatory=$true)][String]$DatabaseName,
  [Parameter(Mandatory=$true)][String]$CollectionName,
  [Parameter(Mandatory=$true)][String]$Query
)

Function GenerateAuthorizationHeader{
  [CmdletBinding()]
  Param(
      [Parameter(Mandatory=$true)][String]$verb,
      [Parameter(Mandatory=$true)][String]$resourceLink,
      [Parameter(Mandatory=$true)][String]$resourceType,
      [Parameter(Mandatory=$true)][String]$dateTime,
      [Parameter(Mandatory=$true)][String]$key,
      [String]$keyType = "master",
      [String]$tokenVersion = 1.0
  )
  $hmacSha256 = New-Object System.Security.Cryptography.HMACSHA256
  $hmacSha256.Key = [System.Convert]::FromBase64String($key)

  $payLoad = "$($verb.ToLowerInvariant())`n$($resourceType.ToLowerInvariant())`n$resourceLink`n$($dateTime.ToLowerInvariant())`n`n"
  $hashPayLoad = $hmacSha256.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($payLoad))
  $signature = [System.Convert]::ToBase64String($hashPayLoad)

[System.Web.HttpUtility]::UrlEncode("type=$keyType&ver=$tokenVersion&sig=$signature")
}

$dateTime = [DateTime]::UtcNow.ToString("r")
$CosmosAccountName = az cosmosdb list --query "[?documentEndpoint=='$CosmosEndpoint'].name" -o tsv
$ResourceGroup = az cosmosdb list --query "[?name=='$CosmosAccountName'].resourceGroup" -o tsv
$MasterKey = az cosmosdb keys list -n $CosmosAccountName -g $ResourceGroup --query "primaryMasterKey" -o tsv

$authHeader = GenerateAuthorizationHeader -verb post -resourceLink "dbs/$DatabaseName/colls/$CollectionName" -resourceType docs -key $MasterKey -keyType "master" -tokenVersion "1.0" -dateTime $dateTime

$body = @{
  "query"=$Query
} | ConvertTo-Json

$headers = @{
  "x-ms-date"="$dateTime"
  "x-ms-version"="2018-12-31"
  "Authorization"="$authHeader"
  "x-ms-documentdb-isquery"="true"
  "content-type"="application/query+json"
  "x-ms-documentdb-query-enablecrosspartition"="true"
}
 
 Invoke-RestMethod -Uri "https://$CosmosAccountName.documents.azure.com/dbs/$DatabaseName/colls/$CollectionName/docs" -Method 'Post' -Body $body -Headers $headers | ConvertTo-Json
