# Service Principal Data Dictionary 
![License](https://img.shields.io/badge/license-MIT-green.svg)

##Data  
 
The system will have 3 collections of data: ObjectTracking, Audit, and Configuration.  Additionally, a data structure will be stored in the AAD Service Principal Notes field. 

Document Type | Fields | Type| Source
------------ | ------------- | ------------- | -------------
Graph Object |  
 | | id | string/GUID | AAD
| | createdDateTime | datetime | AAD
| | deletedDateTime | datetime | AAD
| | lastUpdatedDateTime |datetime | SPAutomation
| | appId | string/GUID | AAD
| | appDisplayName | string | AAD
| | displayName | string | AAD
| | objectType | string | AAD
| | notes | string | AAD
| | status | string | SPAutomation

##Audit 

Document Type | Fields | Type| Source
------------ | ------------- | ------------- | -------------
Graph Object |  
| | id | string/GUID | AAD
| | createdDateTime | datetime | AAD
| | deletedDateTime | datetime | AAD
| | lastUpdatedDateTime |datetime | SPAutomation
| | appId | string/GUID | AAD
| | appDisplayName | string | AAD
| | displayName | string | AAD
| | objectType | string | AAD
| | notes | string | AAD
| | status | string | SPAutomation
| | correlationId | string/GUID | Application
| | actionType| string | Application
| | actionReason |string | Application
| | actionDateTime | datetime | AAD


##Configuration
Field | Type | Range
------------ | ------------- | ------------- 
| deltaQuery | string |
| runState| string | seedOnly,seedAndRun, deltaRun, disabled
|lastDeltaRun | DateTime |  
| seedTime| DateTime |
 
 
##Notes
Field | Type | Range
------------ | ------------- | ------------- 
| businessOwners | string[] | Valid AAD User


