![License](https://img.shields.io/badge/license-MIT-green.svg)
# Service Principal Data Dictionary 

## Data  

The system will have 3 collections of data: ObjectTracking, Audit, and Configuration.  Additionally, a data structure will be stored in the AAD Service Principal Notes field. 

> REWORK THIS

Attribute | Type | Source
--------- | ---- | ------
id | string/GUID | AAD
createdDateTime | datetimeoffset | AAD
deletedDateTime | datetimeoffset | AAD
lastUpdatedDateTime |datetimeoffset | SPAutomation
appId | string/GUID | AAD
appDisplayName | string | AAD
displayName | string | AAD
objectType | string | AAD
notes | string | AAD
status | string | SPAutomation

## Audit 

Attribute | Type | Source
---------- | ---- | ------
id | string/GUID | AAD
createdDateTime | datetimeoffset | AAD
deletedDateTime | datetimeoffset | AAD
lastUpdatedDateTime |datetimeoffset | SPAutomation
appId | string/GUID | AAD
appDisplayName | string | AAD
displayName | string | AAD
objectType | string | AAD
notes | string | AAD
status | string | SPAutomation
correlationId | string/GUID | Application
actionType| string | Application
actionReason |string | Application
actionDateTime | datetimeoffset | AAD


## Configuration
> REWORK THIS

Attribute | Type | Source
---------- | ---- | ------
deltaQuery | string |
runState | string | seed, deltaRun, disabled
lastDeltaRun | datetimeoffset |  
seedTime | datetimeoffset |
 
 
## ServicePrincipal
Attribute | Type | Source
---------- | ---- | ------
Notes | string | List of Valid AAD Users, delimited (,;)

