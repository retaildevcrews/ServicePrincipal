using System;
using System.Collections.Generic;
using System.Text;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.AuditResults
{
    internal class AuditValidationManager : IResults, IDisposable
    {
        private InputGenerator _inputGenerator;

        public AuditValidationManager(InputGenerator inputGenerator)
        {
            _inputGenerator = inputGenerator;
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
