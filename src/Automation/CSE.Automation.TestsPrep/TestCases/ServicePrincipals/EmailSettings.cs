using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace CSE.Automation.TestsPrep.TestCases.ServicePrincipals
{
    internal class EmailSettings : IEmailSettings, IDisposable
    {
        private IConfigurationRoot _config;
        public string TestEmailBase
        {
            get
            {
                return _config["testEmailBase"];
            }
        }

        public bool IncludeRandomStringToTestEmail
        {
            get
            {
                return false;
            }
        }

        public int TestEmailCount
        {
            get
            {
                return int.Parse(_config["testEmailCount"]);
            }
        }


        public List<string> TestEmailDomainNames => _config["testEmailDomainNames"].Split(',').Select(s => s.Trim()).ToList();


        public EmailSettings(IConfigurationRoot config)
        {
            _config = config;
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
