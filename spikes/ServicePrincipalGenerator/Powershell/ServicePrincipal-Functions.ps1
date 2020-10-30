function Create-ServicePrincipals
{
	[CmdletBinding()]
	param (
		[Parameter(Mandatory=$true)]
		[int]$Count,

		[Parameter(Mandatory=$true)]
		[string]$Prefix,

		[string]$BaseName = "Test"
	)

	$logtime = (Get-Date).ToString("yyyyMMdd-HHmmss")
	$logfilename = "ServicePrincipal-${logtime}.log"

	Start-Transcript $logfilename -IncludeInvocationHeader

	1..$Count | % { 
		$spName = "http://${Prefix}-${BaseName}-$_"
		try
		{
			$sp = az ad sp create-for-rbac --skip-assignment --name  $spName
			if (($_ % 100) -eq 0)
			{
				Write-Output $_
			}
		}
		catch
		{
			Write-Output "Failed to create ServicePrincipal ${spName}"
		}
	}
	
	Stop-Transcript
}