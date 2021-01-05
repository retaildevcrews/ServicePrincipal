# Service Principal Overview 

![License](https://img.shields.io/badge/license-MIT-green.svg)

Enterprises need the ability to identify Business Owners for Service Principals and enforce valid assignments to comply with security requirements from their audit. The current Owners object on Service Principals will not suffice as adding a user to the list grants permissions to the object that are undesired.

It also will help remediate security issues by ensuring Business Owners are associated with Service Principals. 

The purpose of this system is to monitor changes to ServicePrincipals in Azure AD, checking to see if each ServicePrincipal has a valid list of Business Owners, and producing an audit trail.  If a ServicePrincipal is evaluated and FAILs an audit control, the system attempts to remediate the error.  

The Notes field is used for the Business Owners list instead of the Owners attribute.  Since the Notes field is a _text_ attribute, not additional permissions are granted to the parent object by the contents of the attribute.  The Notes field must contain a delimited list (',', ';') of User Principal Names within the same directory as the object.  