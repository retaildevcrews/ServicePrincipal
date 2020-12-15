#!/bin/bash
mdPaths=$(git diff --name-only --cached | grep '.md$')
if [ $(echo $mdPaths | wc -l) -eq 0 ]; then
  exit 0
fi
for mdPath in $mdPaths; do
	mdDir=${mdPath%/*}
	echo '<div>' > /tmp/wrapped_mmd
	cat $mdPath >> /tmp/wrapped_mmd
	echo '</div>' >> /tmp/wrapped_mmd
	renderCount=$(xmllint --xpath 'count(//div[@class="mermaid"])' /tmp/wrapped_mmd)
	if [ $renderCount -eq 0 ]; then
		exit 0
	fi
	for i in $(seq 1 $renderCount); do
		svgPath=$mdDir/$(xmllint --xpath "//div[@class='mermaid'][$i]/text()" /tmp/wrapped_mmd | xargs | perl -pe 's/.*\((.*)\).*/\1/g')
		xmllint --xpath "//div[@class='mermaid'][$i]/details/text()" /tmp/wrapped_mmd | perl -MHTML::Entities -pe 'decode_entities($_);' | sed -n '/mermaid/,/```/p' | sed '1d;$d' > temp
		docker run -v ${PWD}:/mnt/mmd minlag/mermaid-cli:latest -i /mnt/mmd/temp -o /mnt/mmd/$svgPath -c /mnt/mmd/spikes/MermaidMarkdownRender/mermaidConfig.json
		temp=$(cat $svgPath | perl -pe 's/mermaid-\d+/mermaid/g')
		echo $temp > $svgPath
		git add $svgPath
	done
done
rm temp
