#!/usr/bin/python

import sys, getopt
import os

def main(argv):

    prodEnvDict = {}
    prodEnvDict['RESOURCE_GROUP_NAME']  = "serviceprincipal-rg-dev"
    prodEnvDict['FUNCTION_APP_NAME']    = "sp-funcn-dev"

    prodDict = {}

    prodDict['KEYVAULT_NAME'] = "sp-kv-dev"

    prodDict['SPAuditCollection']           = "Audit"
    prodDict['SPConfigurationCollection']   = "Configuration"
    prodDict['SPObjectTrackingCollection']  = "ObjectTracking"

    prodDict['SPDeltaDiscoverySchedule']    = "\"0 */30 * * * *\""

    prodDict['SPEvaluateQueue'] = "evaluate"
    prodDict['SPUpdateQueue']   = "update"

    prodDict['ScanLimit'] = "2000"

    devEnvDict = {}
    devEnvDict['RESOURCE_GROUP_NAME']  = "serviceprincipal-rg-dev"
    devEnvDict['FUNCTION_APP_NAME']    = "sp-funcn-dev"

    devDict ={}

    devDict['KEYVAULT_NAME'] = "sp-kv-dev"

    devDict['SPAuditCollection']           = "Audit"
    devDict['SPConfigurationCollection']   = "Configuration"
    devDict['SPObjectTrackingCollection']  = "ObjectTracking"

    devDict['SPDeltaDiscoverySchedule']    = "\"0 */30 * * * *\""

    devDict['SPEvaluateQueue'] = "evaluate"
    devDict['SPUpdateQueue']   = "update"

    devDict['ScanLimit'] = "2000"



    try:
        mode = sys.argv[1]
    except getopt.GetoptError:
        print("appConfigSetting.py <mode>, where mode = prod | qa")
        sys.exit(2)
    
    if mode == 'prod':

        print("Applying Prod environment app function config settings")

        for setting in prodDict:
            print(f"Updating {setting}")
            print(f"az functionapp config appsettings set --name {prodEnvDict['FUNCTION_APP_NAME']} --resource-group {prodEnvDict['RESOURCE_GROUP_NAME']} --settings {setting}={prodDict[setting]}")
            os.system(f"az functionapp config appsettings set --name {prodEnvDict['FUNCTION_APP_NAME']} --resource-group {prodEnvDict['RESOURCE_GROUP_NAME']} --settings {setting}={prodDict[setting]}")
            
    elif mode == 'qa':

        print ("Applying QA environment app function config settings")

        for setting in prodDict:
            print(f"Updating {setting}")
            print(f"az functionapp config appsettings set --name {prodEnvDict['FUNCTION_APP_NAME']} --resource-group {prodEnvDict['RESOURCE_GROUP_NAME']} --settings {setting}={prodDict[setting]}")
            os.system(f"az functionapp config appsettings set --name {prodEnvDict['FUNCTION_APP_NAME']} --resource-group {prodEnvDict['RESOURCE_GROUP_NAME']} --settings {setting}={prodDict[setting]}")

    else:
        print("appConfigSetting.py <mode>, where mode = prod | qa")
        sys.exit(2)



if __name__ == "__main__":
   main(sys.argv[1:])