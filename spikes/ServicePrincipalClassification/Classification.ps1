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
