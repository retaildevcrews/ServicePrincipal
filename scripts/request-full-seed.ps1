# setup
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

$FN_NAME = "fa-svcprincipal-cse-dev"

$RESOURCE_GROUP=$(az functionapp list --query "[?name=='$FN_NAME']" --query "[].resourceGroup" -o tsv)

# post to cosmosdb

$CosmosEndpoint=$(az functionapp config appsettings list -n $FN_NAME -g $RESOURCE_GROUP --query "[?name=='SPCosmosURL'][].value" -o tsv)

$CosmosDatabase=$(az functionapp config appsettings list -n $FN_NAME -g $RESOURCE_GROUP --query "[?name=='SPCosmosDatabase'][].value" -o tsv)

$Collection = $(az functionapp config appsettings list -n $FN_NAME -g $RESOURCE_GROUP --query "[?name=='SPActivityHistoryCollection'][].value" -o tsv)

$correlationId = [System.guid]::NewGuid().toString()
$activityId = [System.guid]::NewGuid().toString()

$CosmosAccountName = az cosmosdb list --query "[?documentEndpoint=='$CosmosEndpoint'].name" -o tsv
$ResourceGroup = az cosmosdb list --query "[?name=='$CosmosAccountName'].resourceGroup" -o tsv
$MasterKey = az cosmosdb keys list -n $CosmosAccountName -g $ResourceGroup --query "primaryMasterKey" -o tsv

$document = @{
  id = $activityId;
  correlationId = $correlationId;
  created = [System.DateTimeOffset]::Now;
  name = "Full Discovery Request";
  status = "Running"
} | ConvertTo-Json

$now = [DateTime]::UtcNow.ToString("r")

$authHeader = GenerateAuthorizationHeader -verb post -resourceLink "dbs/$CosmosDatabase/colls/$Collection" -resourceType docs -key $MasterKey -keyType "master" -tokenVersion "1.0" -dateTime $now

$headers = @{
  "x-ms-date"=$now
  "x-ms-version"="2018-12-31"
  "Authorization"="$authHeader"
  "x-ms-documentdb-partitionkey"="[`"$correlationId`"]"
}

Invoke-RestMethod -Uri "https://$CosmosAccountName.documents.azure.com/dbs/$CosmosDatabase/colls/$Collection/docs" -Method 'Post' -Body $document -Headers $headers | ConvertTo-Json

# post to discover queue

$StorageConnectionString=$(az functionapp config appsettings list -n $FN_NAME -g $RESOURCE_GROUP --query "[?name=='SPStorageConnectionString'][].value" -o tsv)

$DiscoverQueueName=$(az functionapp config appsettings list -n $FN_NAME -g $RESOURCE_GROUP --query "[?name=='SPDiscoverQueue'][].value" -o tsv)

$message = @{
  timestamp = [System.DateTimeOffset]::Now;
  operation = "FullSeed";
  discoveryMode = "FullSeed";
  activityId = $activityId;
  correlationId = $correlationId
} | ConvertTo-Json
$bytes = [System.Text.Encoding]::Unicode.GetBytes($message)
$encodedMessage =[Convert]::ToBase64String($Bytes)

az storage message put --content $encodedMessage --queue-name $DiscoverQueueName --connection-string $StorageConnectionString

