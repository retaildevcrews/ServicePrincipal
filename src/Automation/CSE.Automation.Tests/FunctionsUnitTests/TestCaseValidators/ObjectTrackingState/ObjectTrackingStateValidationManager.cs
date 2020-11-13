using System;
using System.Collections.Generic;
using System.Text;
using CSE.Automation.DataAccess;
using CSE.Automation.Model;
using Microsoft.Graph;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.InputGenerator;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ObjectTrackingState
{
    class ObjectTrackingStateValidationManager : IDisposable
    {

        private readonly InputGenerator _inputGenerator;
        private readonly ObjectTrackingRepository _objectTrackingRepository;
        private readonly ActivityContext _activityContext;

        public ObjectTrackingStateValidationManager(InputGenerator inputGenerator, ObjectTrackingRepository objectTrackingRepository, ActivityContext activityContext)
        {
            _inputGenerator = inputGenerator;
            _objectTrackingRepository = objectTrackingRepository;
            _activityContext = activityContext;
        }

        public bool ValidatePrecondition()
        {
            ServicePrincipal servicePrincipal = _inputGenerator.GetServicePrincipal();
            ServicePrincipalModel servicePrincipalModel = _inputGenerator.GetServicePrincipalModel();

            TestCase testCase = _inputGenerator.TestCaseId;

            string objectStateDefinitionClassName = testCase.GetObjectStateDefinition();

            string objectToInstantiate = $"CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ObjectTrackingState.{objectStateDefinitionClassName}, CSE.Automation.Tests";

            var objectType = Type.GetType(objectToInstantiate);

            object[] args = { servicePrincipal , servicePrincipalModel, _objectTrackingRepository, _activityContext};

            var instantiatedObject = Activator.CreateInstance(objectType, args) as IObjectStateDefinition;

            return instantiatedObject.Validate();
        }
        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
