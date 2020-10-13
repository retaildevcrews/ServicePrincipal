##########################################################
# PLEASE READ:
# 
# This script will be loaded by Visual Studio when the
# solution is loaded.  To force a refresh of this script,
# issue the Update-SolutionScripts command in Package
# Manager Console.
##########################################################
#
# Powershell script for adding/removing/showing entries to the hosts file.
# https://gist.github.com/markembling/173887/

$global:StorageEmulatorLocation = "${Env:ProgramFiles(x86)}\microsoft sdks\azure\storage emulator\AzureStorageEmulator.exe"
$global:CosmosDBExeLocation = "${Env:ProgramW6432}\Azure Cosmos DB Emulator\Microsoft.Azure.Cosmos.Emulator.exe"
$global:CosmosDBUri = "https://localhost:8081/_explorer/index.html"

function global:Setup-Environment()
{
	Check-StorageEmulator
	Check-CosmosDBEmulator
}

function global:Start-StorageEmulator()
{
	$status = & $global:StorageEmulatorLocation "status"
	if ($status -contains 'IsRunning: False') {
		Write-Host "starting queue"
		& $global:StorageEmulatorLocation "start" 
	 }
}

function global:Stop-StorageEmulator()
{
	$status = & $global:StorageEmulatorLocation "status" 
	if ($status -contains 'IsRunning: True') {
		Write-Host "starting queue"
		& $global:StorageEmulatorLocation "stop" 
	 }
}

function global:Start-CosmosDB()
{
	try
	{
		Invoke-WebRequest -UseBasicParsing -Uri $global:CosmosDBUri -TimeoutSec 3 | Out-Null
		Write-Output "CosmosDB is running"
	}
	catch
	{
		Write-Output "CosmosDB is not running, starting...."
		& $global:CosmosDBExeLocation 
		Write-Output "`tDone."
	}
}

function global:Stop-CosmosDB()
{
	try
	{
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


function global:Check-StorageEmulator()
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

function global:Check-CosmosDBEmulator()
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

Write-Output "SolutionCommands loaded."