
[CmdletBinding()]
Param ()

Write-Host "Mermaid Diagram Generation Hook"


$mdPaths = git diff --name-only --cached | Select-String -Pattern ".md"
Write-Verbose "$($mdPaths.Count) paths found containing an .md file"
Write-Verbose $($mdPaths -join ', ')

if ($mdPaths.Count -gt 0)
{
  
  $mdPaths |
    ForEach-Object {
      $mdDir = Split-Path $_

      $mdDir = $mdDir -Replace "\\","/"
      Write-Verbose "Checking $mdDir"
      
      $wrappedMd = "<div>`n"
      $wrappedMd += (Get-Content $mdPaths[0]) -join "`n"
      $wrappedMd += "`n</div>"
      $xml = [xml]$wrappedMd
      $xml.div.div |
        Where-Object { $_.class -eq "mermaid" } |
        ForEach-Object {
          if ($_.'#text' -match '\((.+)\)') 
          {
            $_.details.'#text' | ForEach-Object {$_ -replace '```mermaid|```', ''} |
                    docker run -i -v "$(Get-Location):/mnt/mmd" minlag/mermaid-cli:latest -o "/mnt/mmd/$mdDir/$($matches[1])" -c /mnt/mmd/.github/hooks/mermaidConfig.json

            $svgPath = Join-Path -Path $mdDir -ChildPath $matches[1]

            Write-Verbose "Writing new content for $svgPath"
            (Get-Content $svgPath) | ForEach-Object {$_ -replace 'mermaid-\d+', 'mermaid'} | Set-Content -Path $svgPath
            
            git add $svgPath
          }
        }
    }
}
