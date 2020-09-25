using System;
using System.Collections.Generic;
using System.Text;
using CSE.Automation;

namespace CSE.Automation.Interfaces
{
    public interface IServiceResolver
    {
        T GetService<T>(string keyName);
    }

}
