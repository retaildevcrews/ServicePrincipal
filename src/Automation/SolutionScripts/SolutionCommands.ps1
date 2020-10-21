##########################################################
# PLEASE READ:
# 
# This script will be loaded by Visual Studio when the
# solution is loaded.  To force a refresh of this script,
# issue the Update-SolutionScripts command in Package
# Manager Console.
##########################################################
#

# default location for storage emulator
$global:StorageEmulatorLocation = "${Env:ProgramFiles(x86)}\microsoft sdks\azure\storage emulator\AzureStorageEmulator.exe"

# default install path for emulator
$global:CosmosDBRootLocation = "${Env:ProgramW6432}\Azure Cosmos DB Emulator"

# default location for emulator 
$global:CosmosDBExeLocation = Join-Path $global:CosmosDBRootLocation "Microsoft.Azure.Cosmos.Emulator.exe"

# default URI for emulator
$global:CosmosDBRootUri = "https://localhost:8081"	

# root page for emulator
$global:CosmosDBUri = "${global:CosmosDBRootUri}/_explorer/index.html"	

# well known emulator key
$global:CosmosKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw=="   

# probable storage key
$global:StorageKey = "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw=="

# default storage account name
$global:StorageAccount = "devstoreaccount1"

# Storage Queue Endpoint Version
$global:StorageVersion = "2019-12-12"

function global:Setup-Environment()
{
	Ensure-StorageEmulatorInstalled
	Ensure-CosmosDBEmulatorInstalled

	Start-StorageEmulator
	Start-CosmosDB

	ProvisionLocalResources
}

# This function is solution specific - it provisions local resources specific for this application solution.
function global:ProvisionLocalResources()
{
	ProvisionCosmosResources
	ProvisionStorageResources
}

function global:ProvisionStorageResources(){
	$queues = "evaluate"#, "update"
	
	# Creates queues
	$queues | % {
		$url = "http://127.0.0.1:10001/${StorageAccount}/$_"
		$utc_now = (Get-Date).ToUniversalTime().ToString("R")
		$signature = GenerateStorageAuthToken "PUT" "" $utc_now
		#Write-Output "Signature: $signature "
		$authHeader = "SharedKey ${StorageAccount}:${signature}"
		Write-Output $authHeader
		$headers = @{
			Authorization = $authHeader;
			"x-ms-date" = $utc_now;
			"x-ms-version" = $StorageVersion
		}
	  #Write-Output "Headers: $($headers.Keys | % ToString) "
	  #Write-Output "Headers: $($headers.Values | % ToString) "
		Invoke-WebRequest -UseBasicParsing -Method PUT -Uri $url -Headers $headers
	}
}

function global:ProvisionCosmosResources(){
	$databaseName = "SPAutomate"
	$collections = @{
		"Configuration"= "/configType";
		"Audit" = "/actionMonthYear";
		"ObjectTracking" = "/objectType";
	}

	$databases = GetCosmosDatabases
	if (($databases | select -ExpandProperty Id) -notcontains $databaseName)
	{
		Write-Output "Database $databaseName not found, creating."
		CreateCosmosDatabase $databaseName | Out-Null
	}

	$existingCollections = GetCosmosDatabaseCollections $databaseName
	$collectionNames = $existingCollections | select -ExpandProperty Id
	$collections | % getEnumerator | % { 
		if ($collectionNames -contains $_.key)
		{
			Write-Output "Collection $($_.key) exists."
		}
		else
		{
			Write-Output "Collection $($_.key) not found, creating."
			CreateCosmosDatabaseCollection $databaseName $_.key $_.value | Out-Null
		}
	}
}

function global:Start-StorageEmulator()
{
	$status = & $global:StorageEmulatorLocation "status"
	if ($status -contains 'IsRunning: False') {
		Write-Host "Storage Emulator is not running, starting...."
		& $global:StorageEmulatorLocation "start" 
		Write-Output "`tDone."
	 }
	 else
	 {
		  Write-Output "Storage Emulator is running."
	 }
}

function global:Stop-StorageEmulator()
{
	$status = & $global:StorageEmulatorLocation "status" 
	if ($status -contains 'IsRunning: True') {
		Write-Host "Stopping Storage Emulator"
		& $global:StorageEmulatorLocation "stop" 
	 }
}

function global:Start-CosmosDB()
{
	try
	{
		Import-Module (Join-Path $global:CosmosDBRootLocation "PSModules\Microsoft.Azure.CosmosDB.Emulator")

		# we could also use Get-CosmosDbEmulatorStatus
		Invoke-WebRequest -UseBasicParsing -Uri $global:CosmosDBUri -TimeoutSec 3 | Out-Null
		Write-Output "CosmosDB is running"
	}
	catch
	{
		Write-Output "CosmosDB is not running, starting...."
		# this is a standard key for the emulator
		& $global:CosmosDBExeLocation 
		Write-Output "`tDone."
	}
}

function global:Stop-CosmosDB()
{
	try
	{
		Import-Module (Join-Path $global:CosmosDBRootLocation "PSModules\Microsoft.Azure.CosmosDB.Emulator")

		# we could also use Get-CosmosDbEmulatorStatus
		Invoke-WebRequest -UseBasicParsing -Uri $global:CosmosDBUri -TimeoutSec 3 -ErrorAction SilentlyContinue | Out-Null
		Write-Output "CosmosDB is running"
		& $global:CosmosDBExeLocation "/Shutdown"
		Write-Output "CosmosDB is shutting down."
	}
	catch
	{
		Write-Output "CosmosDB is not running."
	}
}


function global:Ensure-StorageEmulatorInstalled()
{
	param (
		[switch]$TestOnly
	)

	$downloadUri = "https://go.microsoft.com/fwlink/?linkid=717179&clcid=0x409"
	if (-not (Test-Path $StorageEmulatorLocation))
	{
		if ($TestOnly) { return $false }

		Write-Output "Storage Emulator is not installed, installing now."

		$fileName = "microsoftazurestorageemulator.msi"
		$tempFolderPath = Join-Path $Env:TEMP $(New-Guid)
		$execFileName = Join-Path $tempFolderPath $fileName
		New-Item -Type Directory -Path $tempFolderPath | Out-Null
		Invoke-WebRequest -UseBasicParsing -Method GET -Uri $downloadUri -OutFile $execFileName

		Install-MSI $fileName $execFileName
	}

	if ($TestOnly) { return $true }
}

function global:Ensure-CosmosDBEmulatorInstalled()
{
	param (
		[switch]$TestOnly
	)

	$downloadUri = "https://aka.ms/cosmosdb-emulator"
	if (-not (Test-Path $global:CosmosDBExeLocation))
	{
		if ($TestOnly) { return $false }

		Write-Output "CosmosDB Emulator is not installed, installing now."

		$fileName = "cosmosdbemulator.msi"
		$tempFolderPath = Join-Path $Env:TEMP $(New-Guid)
		$execFileName = Join-Path $tempFolderPath $fileName
		New-Item -Type Directory -Path $tempFolderPath | Out-Null
		Invoke-WebRequest -UseBasicParsing -Method GET -Uri $downloadUri -OutFile $execFileName

		Install-MSI $fileName $execFileName
	}

	Import-Module (Join-Path $global:CosmosDBRootLocation "PSModules\Microsoft.Azure.CosmosDB.Emulator")

	if ($TestOnly) { return $true }
}


function global:Install-MSI()
{
	param (
		[string]$fileName,
		[string]$execFileName
	)
	$timestamp = get-date -Format yyyyMMddTHHmmss
	$logFile = '{0}\{1}-{2}.log' -f $Env:TEMP,$fileName,$timestamp
	$MSIArguments = @(
		"/i"
		('"{0}"' -f $execFileName)
		"/qn"
		"/norestart"
		"/L*v"
		$logFile
	)
	Start-Process "msiexec.exe" -ArgumentList $MSIArguments -Wait -NoNewWindow 
}

function global:GetCosmosDatabases()
{
	$uri = "${global:CosmosDBRootUri}/dbs"

	$utc_now = (Get-Date).ToUniversalTime().ToString("R").ToLower()

	$verb = "GET"
	$resourceType = "dbs"
	$resourceId = ""
	
	$authHeader = global:GenerateCosmosAuthToken $verb $resourceType $resourceId $utc_now
	$headers = @{
		"x-ms-date" = $utc_now;
		"x-ms-version" = "2015-08-06";
		Authorization = $authHeader
	}

	$result = Invoke-WebRequest -UseBasicParsing -Method GET -Uri $uri -Headers $headers
	$databases = ($result.Content | ConvertFrom-Json).Databases
	return ,$databases
}

function global:GetCosmosDatabaseCollections()
{
	[CmdletBinding()]
	param (
		[Parameter(Mandatory=$true)]
		[string]$databaseName
	)

	$uri = "${global:CosmosDBRootUri}/dbs/$databaseName/colls"

	$utc_now = (Get-Date).ToUniversalTime().ToString("R").ToLower()

	$verb = "GET"
	$resourceType = "colls"
	$resourceId = "dbs/$databaseName"
	
	$authHeader = global:GenerateCosmosAuthToken $verb $resourceType $resourceId $utc_now
	$headers = @{
		"x-ms-date" = $utc_now;
		"x-ms-version" = "2015-08-06";
		Authorization = $authHeader
	}

	$result = Invoke-WebRequest -UseBasicParsing -Method GET -Uri $uri -Headers $headers
	$collections = ($result.Content | ConvertFrom-Json).DocumentCollections
	return ,$collections
}

function global:CreateCosmosDatabase()
{
	param (
		[Parameter(Mandatory=$true)]
		[string]$databaseName
	)

	$uri = "${global:CosmosDBRootUri}/dbs"

	$utc_now = (Get-Date).ToUniversalTime().ToString("R").ToLower()

	$verb = "POST"
	$resourceType = "dbs"
	$resourceId = ""

	$authHeader = global:GenerateCosmosAuthToken $verb $resourceType $resourceId $utc_now
	$headers = @{
		"x-ms-date" = $utc_now;
		"x-ms-version" = "2015-08-06";
		Authorization = $authHeader;
	}
	$body = "{""id"":""$databaseName""}"
	#Write-Output $body
	$result = Invoke-WebRequest -UseBasicParsing -Method $verb -Uri $uri -Headers $headers -Body $body
	return $result
}

function global:CreateCosmosDatabaseCollection()
{
	param (
		[Parameter(Mandatory=$true)]
		[string]$databaseName,

		[Parameter(Mandatory=$true)]
		[string]$collectionName,

		[Parameter(Mandatory=$true)]
		[string]$partitionKey
	)

	$uri = "${global:CosmosDBRootUri}/dbs/$databaseName/colls"
	$utc_now = (Get-Date).ToUniversalTime().ToString("R").ToLower()

	$verb = "POST"
	$resourceType = "colls"
	$resourceId = "dbs/$databaseName"

	$authHeader = global:GenerateCosmosAuthToken $verb $resourceType $resourceId $utc_now
	$headers = @{
		"x-ms-date" = $utc_now;
		"x-ms-version" = "2015-08-06";
		Authorization = $authHeader;
	}
	$body = "{""id"":""$collectionName"", ""partitionKey"": { ""paths"": [ ""$partitionKey""],""kind"":""Hash"",""version"": 1 } }"
	#Write-Output $body
	$result = Invoke-WebRequest -UseBasicParsing -Method $verb -Uri $uri -Headers $headers -Body $body
	return $result
}

function global:GenerateCosmosAuthToken()
{
	param (
		[string]$verb,
		[string]$resourceType,
		[string]$resourceId,
		[string]$date = ((Get-Date).ToString("R")),
		[string]$key=$global:CosmosKey,
		[string]$keyType="master",
		[string]$tokenVersion="1.0"
	)

	$verb = $verb.ToLowerInvariant()
	$resourceType = $resourceType.ToLowerInvariant()
	$date = $date.ToLowerInvariant()
	#$resourceId = $resourceId.ToLowerInvariant()

	$hmacsha = New-Object System.Security.Cryptography.HMACSHA256
	$hmacsha.key = [System.Convert]::FromBase64String($key)

	$payload = "{0}`n{1}`n{2}`n{3}`n{4}`n" -f $verb, $resourceType, $resourceId, $date, ""
	$signature = $hmacsha.ComputeHash([Text.Encoding]::ASCII.GetBytes($payload))
	$signature = [Convert]::ToBase64String($signature)

	$value = "type={0}&ver={1}&sig={2}" -f $keyType, $tokenVersion, $signature
	$value = [System.Web.HttpUtility]::UrlEncode($value)

	#Write-Output $payload
	#Write-Output $signature
	#Write-Output $value

	return $value
}

function global:GenerateStorageAuthToken($method, $Resource, $GMTTime)
{
	#VERB`nContent-Encoding`nContent-Language`nContent-Length`nContent-MD5`nContent-Type`nDate`nIf-Modified-Since`nIf-Match`nIf-None-Match`nIf-Unmodified-Since`nRange`nCanonicalizedHeaders CanonicalizedResource;
	$canonicalizedHeaders = "x-ms-date:$($GMTTime)`nx-ms-version:$($StorageVersion)`n"
	$canonicalizedResource = "${Resource}"
	$payload = "$($method)`n`n`n`n`n`n`n`n`n`n`n`n$($canonicalizedHeaders)$($canonicalizedResource)"
	#Write-Output $payload
	
	$hmacsha = New-Object System.Security.Cryptography.HMACSHA256
	$hmacsha.key = [System.Convert]::FromBase64String($StorageKey)
	$signature = $hmacsha.ComputeHash([Text.Encoding]::ASCII.GetBytes($payload))
	$signature = [Convert]::ToBase64String($signature)

	return $signature
}

Write-Output "SolutionCommands loaded."
