using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Graph;
using Newtonsoft.Json;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ServicePrincipalResults
{
    class ServicePrincipalValidationManager : IDisposable, IResultsManager
    {
        private InputGenerator _inputGenerator;

        private string _savedServicePrincipalAsString;

        public ServicePrincipalValidationManager(InputGenerator inputGenerator)
        {
            _inputGenerator = inputGenerator;
            SaveState();
        }


        public void SaveState()
        {
            _savedServicePrincipalAsString = JsonConvert.SerializeObject(_inputGenerator.GetServicePrincipal());
        }

        public bool Validate()
        {
            string resultValidatorClassName=  _inputGenerator.TestCaseId.GetSpValidator();
            string objectToInstantiate = $"CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ServicePrincipalResults.{resultValidatorClassName}, CSE.Automation.Tests";

            var objectType = Type.GetType(objectToInstantiate);

            object[] args = { _savedServicePrincipalAsString, _inputGenerator.GetServicePrincipal(true), _inputGenerator.TestCaseId};

            var instantiatedObject = Activator.CreateInstance(objectType, args) as ISpResultValidator;

            return instantiatedObject.Validate();


        } 
        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
