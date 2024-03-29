﻿using System;
using System.Threading.Tasks;
using CSE.Automation.DataAccess;
using CSE.Automation.Model;
using CSE.Automation.Tests.IntegrationTests.TestCaseValidators.ObjectTrackingState;

namespace CSE.Automation.Tests.IntegrationTests.TestCaseValidators.ObjectTrackingResults
{
    internal class ObjectTrackingValidationManager : IResultsManager, IDisposable
    {
        private readonly IInputGenerator _inputGenerator;
        private readonly ObjectTrackingRepository _objectTrackingRepository;
        private readonly ActivityContext _activityContext;

        private TrackingModel _savedTrackingModel;

        public ObjectTrackingValidationManager(IInputGenerator inputGenerator, ObjectTrackingRepository objectTrackingRepository, ActivityContext activityContext)
        {
            _inputGenerator = inputGenerator;
            _objectTrackingRepository = objectTrackingRepository;
            _activityContext = activityContext;

            using var objectTrackingValidationManager = new ObjectTrackingPreconditionValidationManager(inputGenerator, _objectTrackingRepository, activityContext);
            
            //this method wil ensure that  the Objectracking Item in the collection  match the precondition for the corresponding TestCase
            objectTrackingValidationManager.ValidatePrecondition();

            SaveState();
        }

        public void SaveState()
        {
            _savedTrackingModel = GetObjectTrackingModel();
        }
    
        private TrackingModel GetObjectTrackingModel()
        {
            Task<TrackingModel> getObjectTrackingItem = Task.Run(() => _objectTrackingRepository.GetByIdAsync(_inputGenerator.GetServicePrincipal().Id, "ServicePrincipal"));
            getObjectTrackingItem.Wait();

            return getObjectTrackingItem.Result;

        }
        public bool Validate()
        {
            string resultValidatorClassName =  _inputGenerator.TestCaseCollection.GetObjectValidator(_inputGenerator.TestCaseId);  //_inputGenerator.TestCaseId.GetObjectValidator();

            string objectToInstantiate = $"CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ObjectTrackingResults.{resultValidatorClassName}, CSE.Automation.Tests";

            var objectType = Type.GetType(objectToInstantiate);

            var newObjectTrackingModel = GetObjectTrackingModel();

            object[] args = { _savedTrackingModel, newObjectTrackingModel, _activityContext,_inputGenerator.TestCaseId};

            var instantiatedObject = Activator.CreateInstance(objectType, args) as IObjectResultValidator;

            return instantiatedObject.Validate();

        } 
        public void Dispose()
        {
            //throw new NotImplementedException();
        }

    }
}
