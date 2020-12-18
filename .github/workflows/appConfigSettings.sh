#!/bin/bash

environment=$1

if [ $environment = 'prod' ];
then
  configSettingsFile=".github/workflows/appConfigSettingsProd.json"
elif [ $environment = 'qa' ];
then
  configSettingsFile=".github/workflows/appConfigSettingsQA.json"
else
    exit 1
fi

RESOURCE_GROUP_NAME=${RESROUCE_GROUP}
FUNCTION_APP_NAME=${FN_NAME}
echo $RESOURCE_GROUP_NAME 
echo $FUNCTION_APP_NAME 



cat $configSettingsFile | \
  jq -r 'to_entries | map(.key + "=" + .value) | @tsv' | \
  tr "\t" "\n" | \
  (cat && echo "KEYVAULT_NAME=${KV_NAME}") | \
  while read line 
do
  if [[ $line == "RESOURCE_GROUP_NAME"* ]]; then
    continue
  elif [[ $line == "FUNCTION_APP_NAME"* ]]; then
    continue
  fi 
  echo "$line"
  if [ $environment = 'prod' ];
  then
    az functionapp config appsettings set --name ${FUNCTION_APP_NAME} --resource-group ${RESOURCE_GROUP_NAME} --settings "$line" > /dev/null 2>&1
  elif [ $environment = 'qa' ];
  then
    az functionapp config appsettings set --name ${FUNCTION_APP_NAME} --resource-group ${RESOURCE_GROUP_NAME} --slot staging --settings "$line" > /dev/null 2>&1
  fi
done
