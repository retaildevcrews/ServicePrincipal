[CmdletBinding()]
Param(
  [ValidateSet("FullSeed", "Deltas")]
  [string]$DiscoveryMode = "FullSeed",

  [string]$FunctionName = "fa-svcprincipal-cse-dev",

  [string]$QueueName = $null,

  [switch]$Local
)

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

# setup our global ids
$correlationId = [System.guid]::NewGuid().toString()
$activityId = [System.guid]::NewGuid().toString()

if ($Local)
{
  # executing in local mode, using development storage (for testing)
  $StorageConnectionString="UseDevelopmentStorage=true"
  # executing in local mode, using well-known queue name (for testing)
  if ([string]::IsNullOrWhiteSpace($QueueName))
  {
    $QueueName="discover"
  }  
}
else {
  # Create Activity record in CosmosDB (for tracking)
  $RESOURCE_GROUP=$(az functionapp list --query "[?name=='$FunctionName']" --query "[].resourceGroup" -o tsv)

  $CosmosEndpoint=$(az functionapp config appsettings list -n $FunctionName -g $RESOURCE_GROUP --query "[?name=='SPCosmosURL'][].value" -o tsv)
  $CosmosDatabase=$(az functionapp config appsettings list -n $FunctionName -g $RESOURCE_GROUP --query "[?name=='SPCosmosDatabase'][].value" -o tsv)
  $Collection = $(az functionapp config appsettings list -n $FunctionName -g $RESOURCE_GROUP --query "[?name=='SPActivityHistoryCollection'][].value" -o tsv)

  $CosmosAccountName = az cosmosdb list --query "[?documentEndpoint=='$CosmosEndpoint'].name" -o tsv
  $ResourceGroup = az cosmosdb list --query "[?name=='$CosmosAccountName'].resourceGroup" -o tsv
  $MasterKey = az cosmosdb keys list -n $CosmosAccountName -g $ResourceGroup --query "primaryMasterKey" -o tsv

  $document = @{
    id = $activityId;
    correlationId = $correlationId;
    created = [System.DateTimeOffset]::Now;
    name = "Discovery Request";
    status = "Completed"
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

  Write-Host "Activity Recorded - Cosmos: $($CosmosAccountName), DB: $($CosmosDatabase), CorrelationId: $($correlationId)"

  # if we are non-local, find the storage connection string from the function app
  $StorageConnectionString=$(az functionapp config appsettings list -n $FunctionName -g $RESOURCE_GROUP --query "[?name=='SPStorageConnectionString'][].value" -o tsv)

  # if no queue name is provided, query the function app for its Discover queue setting
  if ([string]::IsNullOrWhiteSpace($QueueName))
  {
    $QueueName=$(az functionapp config appsettings list -n $FunctionName -g $RESOURCE_GROUP --query "[?name=='SPDiscoverQueue'][].value" -o tsv)
  }  
}


Write-Host "QueueName: $QueueName"
# Command the Discovery by posting message directly into queue


# $message = @{
#   timestamp = [System.DateTimeOffset]::Now;
#   operation = "FullSeed";
#   discoveryMode = "FullSeed";
#   activityId = $activityId;
#   correlationId = $correlationId
# } | ConvertTo-Json
$Source = "PSH"
$message = @{
  QueueMessageType = 0;  # Data
  Document = @{
    CorrelationId = $correlationId;
    DiscoveryMode = $DiscoveryMode;
    Source = $Source;
  };
  Attempt = 1;
} | ConvertTo-Json -Compress

# Must use UTF8 encoding
$bytes = [System.Text.Encoding]::UTF8.GetBytes($message)
$encodedMessage =[Convert]::ToBase64String($Bytes)

# the auth and queue name parameters seem to be order dependent
az storage message put --connection-string $StorageConnectionString --queue-name $QueueName --content $encodedMessage 

Write-Host "Message Sent - Queue: $($QueueName)"
Write-Host "Message: $($message)"

