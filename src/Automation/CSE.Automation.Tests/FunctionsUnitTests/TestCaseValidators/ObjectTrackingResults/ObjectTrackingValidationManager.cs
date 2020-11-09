using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using CSE.Automation.DataAccess;
using CSE.Automation.Model;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ObjectTrackingResults
{
    internal class ObjectTrackingValidationManager : IResultsManager, IDisposable
    {
        private readonly InputGenerator _inputGenerator;
        private readonly ObjectTrackingRepository _objectTrackingRepository;

        private TrackingModel _savedTrackingModel;

        public ObjectTrackingValidationManager(InputGenerator inputGenerator, ObjectTrackingRepository objectTrackingRepository)
        {
            _inputGenerator = inputGenerator;
            _objectTrackingRepository = objectTrackingRepository;
            SaveState();
        }

        public void SaveState()
        {
            Task<TrackingModel> getObjectTrackingItem = Task.Run(() => _objectTrackingRepository.GetByIdAsync(_inputGenerator.GetServicePrincipal().Id, "ServicePrincipal"));
            getObjectTrackingItem.Wait();

            _savedTrackingModel = getObjectTrackingItem.Result;
        }
    
        public bool Validate()
        {
            Task<TrackingModel> getNewObjectTrackingItem = Task.Run(() => _objectTrackingRepository.GetByIdAsync(_inputGenerator.GetServicePrincipal().Id, "ServicePrincipal"));
            getNewObjectTrackingItem.Wait();

            string resultValidatorClassName=  _inputGenerator.GetTestCaseId().GetObjectValidator();
            string objectToInstantiate = $"CSE.Automation.FunctionsUnitTests.TestCaseStateValidators.ObjectTrackingResults.{resultValidatorClassName}, CSE.Automation.Tests";

            var objectType = Type.GetType(objectToInstantiate);

            object[] args = { _savedTrackingModel, getNewObjectTrackingItem.Result, _inputGenerator.GetTestCaseId()};

            var instantiatedObject = Activator.CreateInstance(objectType, args) as IObjectResultValidator;

            return instantiatedObject.Validate();

        } 
        public void Dispose()
        {
            throw new NotImplementedException();
        }

    }
}
