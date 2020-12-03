using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using CSE.Automation.Model;
using Microsoft.Graph;

namespace CSE.Automation.Tests.FunctionsUnitTests
{
    class ServicePrincipalWrapper 
    {
        public ServicePrincipal AADServicePrincipal { get; private set; }

        public ServicePrincipalModel SPModel { get; private set; }
        public List<string> AADUsers { get; private set; }
        public bool HasOwners { get; private set; }
        public ServicePrincipalWrapper(ServicePrincipal spObject, List<string> aadUsers, bool hasOwners)
        {
            AADServicePrincipal = spObject;
            AADUsers = aadUsers;
            HasOwners = hasOwners;

            CreateServicePrincipalModel();
        }


        protected void CreateServicePrincipalModel()
        {
            SPModel = new ServicePrincipalModel()
            {
                Id = AADServicePrincipal.Id,
                AppId = AADServicePrincipal.AppId,
                DisplayName = AADServicePrincipal.DisplayName,
                Notes = AADServicePrincipal.Notes,
                Created = DateTimeOffset.Parse(AADServicePrincipal.AdditionalData["createdDateTime"].ToString(), CultureInfo.CurrentCulture),
                Deleted = AADServicePrincipal.DeletedDateTime,
                Owners = HasOwners ? AADUsers : null
            };
        }

    }
}
