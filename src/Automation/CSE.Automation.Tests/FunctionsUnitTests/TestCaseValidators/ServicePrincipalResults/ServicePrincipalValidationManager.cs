using System;
using System.Collections.Generic;
using System.Text;
using CSE.Automation.Model;
using Microsoft.Graph;
using Newtonsoft.Json;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ServicePrincipalResults
{
    class ServicePrincipalValidationManager : IDisposable, IResultsManager
    {
        private IInputGenerator _inputGenerator;

        private string _savedServicePrincipalAsString;

        private readonly ActivityContext _activityContext;

        public ServicePrincipalValidationManager(IInputGenerator inputGenerator, ActivityContext activityContext, bool saveState = true)
        {
            _inputGenerator = inputGenerator;
            _activityContext = activityContext;

            if (saveState)
            {
                SaveState();
            }
        }


        public void SaveState()
        {
            _savedServicePrincipalAsString = JsonConvert.SerializeObject(_inputGenerator.GetServicePrincipal());
        }

        public bool Validate()
        {
            string resultValidatorClassName = _inputGenerator.TestCaseCollection.GetSpValidator(_inputGenerator.TestCaseId);//   _inputGenerator.TestCaseId.GetSpValidator();
            string objectToInstantiate = $"CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ServicePrincipalResults.{resultValidatorClassName}, CSE.Automation.Tests";

            var objectType = Type.GetType(objectToInstantiate);

            object[] args = { _savedServicePrincipalAsString, _inputGenerator, _activityContext};

            var instantiatedObject = Activator.CreateInstance(objectType, args) as ISpResultValidator;

            return instantiatedObject.Validate();


        } 
        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
