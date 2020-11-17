connect-graph -Scopes "Directory.read.all"
$spList = Get-MgServicePrincipal -All

$rules = Get-Content './resources/classification_mapping.json' | ConvertFrom-Json

For ($i=0; $i -lt $spList.Count; $i++){
  $sp = $spList[$i]
  foreach ($rule in $rules) {
    if ('$sp.' + $rule.matchExpression | Invoke-Expression){
      $spList[$i].Tags = $rule.tags
      break
    }
  }
}

$spList | % {
  $sp = $_
  $rules | % { 
    if ('$sp.' + $_.matchExpression | Invoke-Expression) {
      $sp.Tags = $_.tags
      break
    }
  }
}

function ClassifyMicrosoft
{
  [CmdletBinding()]
  param (
    [GroupInfo]$group

  )
}
function ClassifyGroup
{
  [CmdletBinding()]
  param (
    [GroupInfo]$group
  )

  # if $group.Name is well-known Microsoft -> ClassifyMicrosoft()
  # elif $group.Name is "" -> ClassifyLegacy() (ServicePrincipalType == managedidentity => Company, ServicePrincipalType == Legacy => Microsoft, ServicePrincipalType == SocialIdp => Microsoft)
  # else ClassifyThirdParty()
  
}

$groups = $spList....
$groups | 
  % {
      $groupClass = ClassifyGroup $_.Name
  if ($groupClass -eq 'Microsoft') {ClassifyMicrosoft $_}
  elseif ($groupClass -eq 'Legacy') { ClassifyLegacy $_}
  else { ClassifyThirdParty $_}
    
  }
}