#!/bin/bash

environment=$1

if [ $environment = 'prod' ];
then
  configSettingsFile=".github/workflows/appConfigSettingsProd.json"
	slotName = "${FUNCTION_APP_NAME}-prod"
elif [ $environment = 'qa' ];
then
  configSettingsFile=".github/workflows/appConfigSettingsQA.json"
	slotName = "${FUNCTION_APP_NAME}-qa"
else
    exit 1
fi

RESOURCE_GROUP_NAME=$(cat $configSettingsFile | jq -r '.RESOURCE_GROUP_NAME')
FUNCTION_APP_NAME=$(cat $configSettingsFile | jq -r '.FUNCTION_APP_NAME')
echo $RESOURCE_GROUP_NAME 
echo $FUNCTION_APP_NAME 

cat $configSettingsFile | jq -r 'to_entries | map(.key + "=" + .value) | @tsv' | tr "\t" "\n"  | while read line 
do
  if [[ $line == "RESOURCE_GROUP_NAME"* ]]; then
    continue
  elif [[ $line == "FUNCTION_APP_NAME"* ]]; then
    continue
  fi 
  echo "$line"
  az functionapp config appsettings set --name ${FUNCTION_APP_NAME} --resource-group ${RESOURCE_GROUP_NAME} --slot ${slotName} --settings "$line" > /dev/null 2>&1
done
