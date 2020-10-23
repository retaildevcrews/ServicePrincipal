using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace AzQueueTestTool.TestCases.ServicePrincipals
{
    public class ServicePrincipalSettings
    {
        public string ServicePrincipalPrefix
        {
            get
            {
                return ConfigurationManager.AppSettings.Get("prefix");
            }
        }
        public string ServicePrincipalBaseName
        {
            get
            {
                return ConfigurationManager.AppSettings.Get("baseName");
            }
        }

        public string ClientID
        {
            get
            {
                return ConfigurationManager.AppSettings.Get("clientId");
            }
        }

        public string ClientSecret
        {
            get
            {
                return ConfigurationManager.AppSettings.Get("clientSecret");
            }
        }

        public string TenantId
        {
            get
            {
                return ConfigurationManager.AppSettings.Get("tenantId");
            }
        }

        public int NumberOfServicePrincipalToCreate
        {
            get
            {
                return int.Parse(ConfigurationManager.AppSettings.Get("numberOfServicePrincipalToCreate")); 
            }
        }

    }
}
