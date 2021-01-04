![License](https://img.shields.io/badge/license-MIT-green.svg)
# Service Principal Data Dictionary 

## Data  

The system will have 4 persistent collections of data: Configuration, Audit, ObjectTracking and ActivityHistory.  Additionally, a data structure will be stored in the AAD Service Principal Notes field. 

### Configuration

Attribute | Type | Source
--------- | ---- | ------
id | string/GUID | constant
configType | ProcessorType | ServicePrincipal, User
deltaLink | string | Graph API
runState | RunState | Disabled, Seed, DeltaRun
lastDeltaRun | datetimeoffset | 
lastSeedRun | datetimeoffset | 
isProcessorLocked | bool | 

### Audit 

Attribute | Type | Source
---------- | ---- | ------
id | string/GUID | 
correlationId | string/GUID | trigger
objectId | string/GUID | AAD
type | AuditActionType | Pass, Fail, Change, Ignore
code | AuditCode | Pass_ServicePrincipal, Ignore_ServicePrincipalDeleted, Change_ServicePrincipalUpdated, Fail_AttributeValidation, Fail_MissingOWners
reason | string | 
message | string | 
timestamp | datetimeoffset | 
auditYearMonth | string | self
attributeName | string | 
existingAttributeValue | string | AAD
updatedAttributeValue| string | 



### ObjectTracking


Attribute | Type | Source
---------- | ---- | ------
id | string/GUID | 
correlationId | string/GUID | trigger
created | datetimeoffset |  
deleted | datetimeoffset |
lastUpdated | datetimeoffset | 
objectType | ObjectType | ServicePrincipal
entity | object | ServicePrincipal 
 
### ServicePrincipal
Attribute | Type | Source
---------- | ---- | ------
id | string/GUID | AAD
appId | string/GUID | AAD
created | datetimeoffset |  
deleted | datetimeoffset |
objectType | ObjectType | ServicePrincipal
servicePrincipalType | string | AAD
appDisplayName | string | AAD
displayName | string | AAD
owners | list(string) | AAD
notes | string | List of Valid AAD Users, delimited (,;)

