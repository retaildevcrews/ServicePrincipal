
[CmdletBinding()]
Param ()

Write-Host "Mermaid Diagram Generation Hook"


$mdPaths = git diff --name-only --cached | Select-String -Pattern ".md"
Write-Verbose "$($mdPaths.Count) paths found containing an .md file"

if ($mdPaths.Count -gt 0)
{
  Write-Verbose $($mdPaths -join ', ')
  
  $mdPaths |
    ForEach-Object {
      $mdDir = Split-Path $_

      $mdDir = $mdDir -Replace "\\","/"
      Write-Verbose "Checking $mdDir"
      Write-Verbose "`tGetting content of $($mdPaths[0])"
      
      $wrappedMd = "<div>`n"
      $wrappedMd += (Get-Content $mdPaths[0]) -join "`n"
      $wrappedMd += "`n</div>"
      $xml = [xml]$wrappedMd
      $xml.div.div |
        Where-Object { $_.class -eq "mermaid" } |
        ForEach-Object {
          # Check if there is an existing link to an image in the text of the mermaid div, otherwise gen a new file to be linked by user
          $filename = ""
          if ($_.'#text' -match '\((.+)\)') 
          {
            Write-Verbose "Using existing file reference"
            $filename = $matches[1]
          }
          else 
          {
            Write-Verbose "No file reference found, naming file from div.id"
            $filename = "$($_.id).svg"
          }

          if (-not [string]::IsNullOrWhiteSpace($_.id))
          {
            # mermaid text is in the details section of the div
            $_.details.'#text' | ForEach-Object {$_ -replace '```mermaid|```', ''} |
                    docker run -i -v "$(Get-Location):/mnt/mmd" minlag/mermaid-cli:latest -o "/mnt/mmd/$mdDir/$filename" -c /mnt/mmd/.github/hooks/mermaidConfig.json

            $svgPath = Join-Path -Path $mdDir -ChildPath $filename

            Write-Verbose "Writing new content for $svgPath"
            $svgContents = (Get-Content $svgPath) |
              ForEach-Object {$_ -replace 'mermaid-\d+', 'mermaid'} |
              ForEach-Object {[xml]$_}
            $svgContents.svg.height = "auto"
            Set-Content -Path $svgPath -Value $svgContents.svg.OuterXml
            
            Write-Verbose "Adding $svgPath to index"
            git add $svgPath
          
          }
        }
    }
}
