using Azure.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSE.Automation.Interfaces
{
    public interface ICredentialService
    {
        public TokenCredential CurrentCredential { get; }
    }
}
