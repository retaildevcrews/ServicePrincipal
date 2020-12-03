using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace AzQueueTestTool.TestCases.ServicePrincipals
{
    public class EmailSettings : IDisposable
    {

        public string TestEmailBase
        {
            get
            {
                return ConfigurationManager.AppSettings.Get("testEmailBase");
            }
        }

        public bool IncludeRandomStringToTestEmail
        {
            get
            {
                return bool.Parse(ConfigurationManager.AppSettings.Get("includeRandomStringToTestEmail"));
            }
        }

        public int TestEmailCount
        {
            get
            {
                return int.Parse(ConfigurationManager.AppSettings.Get("testEmailCount"));
            }
        }
        

        public List<string> TestEmailDomainNames => ConfigurationManager.AppSettings.Get("testEmailDomainNames").Split(',').Select(s => s.Trim()).ToList();
       
        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
