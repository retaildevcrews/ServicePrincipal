using System;
using System.Collections.Generic;
using System.Text;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ServicePrincipalResults
{
    class ServicePrincipalValidationManager : IDisposable, IResults
    {
        private InputGenerator inputGenerator;

        public ServicePrincipalValidationManager(InputGenerator inputGenerator)
        {
            this.inputGenerator = inputGenerator;
        }


        public void SaveState()
        {
            throw new NotImplementedException();
        }

        public bool Validate()
        {
            throw new NotImplementedException();
        } 
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
