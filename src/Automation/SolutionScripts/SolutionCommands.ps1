##########################################################
# PLEASE READ:
# 
# This script will be loaded by Visual Studio when the
# solution is loaded.  To force a refresh of this script,
# issue the Update-SolutionScripts command in Package
# Manager Console.
##########################################################
#


$global:StorageEmulatorLocation = "${Env:ProgramFiles(x86)}\microsoft sdks\azure\storage emulator\AzureStorageEmulator.exe"
$global:CosmosDBRootLocation = "${Env:ProgramW6432}\Azure Cosmos DB Emulator"
$global:CosmosDBExeLocation = Join-Path $global:CosmosDBRootLocation "Microsoft.Azure.Cosmos.Emulator.exe"
$global:CosmosDBRootUri = "https://localhost:8081"
$global:CosmosDBUri = "${global:CosmosDBRootUri}/_explorer/index.html"
$global:CosmosKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw=="

function global:ProvisionLocalResources()
{
$databaseName = "SPAutomate"
	$collections = @{
		"Config"= "/configType";
		"Audit" = "";
		"ObjectTracking" = "";
	}

	$databases = GetCosmosDatabases
	if ($databases -eq $null)
	{
		Write-Output "No Databases"
		CreateCosmosDatabase $databaseName
	}

	$existingCollections = GetCosmosDatabaseCollections $databaseName
	$collections | % {
		
	}
}

function global:Setup-Environment()
{
	Ensure-StorageEmulatorInstalled
	Ensure-CosmosDBEmulatorInstalled

	Start-StorageEmulator
	Start-CosmosDB

	ProvisionLocalResources
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
	
	$authHeader = global:GenerateAuthToken $verb $resourceType $resourceId $utc_now
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
	
	$authHeader = global:GenerateAuthToken $verb $resourceType $resourceId $utc_now
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
	$resourceId = "dbs/$databaseName"

	$authHeader = global:GenerateAuthToken $verb $resourceType $resourceId $utc_now
	$headers = @{
		"x-ms-date" = $utc_now;
		"x-ms-version" = "2015-08-06";
		Authorization = $authHeader
	}
	$body = "{""id"":""$databaseName""}"
	Write-Output $body
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

	$authHeader = global:GenerateAuthToken $verb $resourceType $resourceId $utc_now
	$headers = @{
		"x-ms-date" = $utc_now;
		"x-ms-version" = "2015-08-06";
		Authorization = $authHeader
	}
	$body = "{""id"":""$collectionName"", ""partitionKey"": { ""paths"": [ ""$partitionKey""],""kind"":""Hash"",""version"": 1 } }"
	#Write-Output $body
	$result = Invoke-WebRequest -UseBasicParsing -Method $verb -Uri $uri -Headers $headers -Body $body
	return $result
}

function global:GenerateAuthToken()
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


Write-Output "SolutionCommands loaded."