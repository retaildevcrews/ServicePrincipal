#Requires -Module PSSodium

[CmdletBinding()]
param (
  [Parameter(Mandatory=$true)][string]$UserName,
  [Parameter(Mandatory=$true)][string]$PersonalToken,
  [Parameter(Mandatory=$true)][string]$OrgAndRepo,
  [Parameter(Mandatory=$true)][string]$SecretKey,
  [Parameter(Mandatory=$true)][string]$SecretVal
)

$base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f $UserName,$PersonalToken)))

$repoPublicKey = Invoke-RestMethod -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)} "https://api.github.com/repos/$OrgAndRepo/actions/secrets/public-key"

$encryptedValue = ConvertTo-SodiumEncryptedString $SecretVal $repoPublicKey.key

$body = @{
  encrypted_value=$encryptedValue
  key_id=$repoPublicKey.key_id
} | ConvertTo-Json

Invoke-RestMethod -Method Put -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)} "https://api.github.com/repos/$OrgAndRepo/actions/secrets/$SecretKey" -Body $body
