using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSE.Automation.DataAccess;
using CSE.Automation.Model;

namespace CSE.Automation.Tests.UnitTests.TestCaseValidators.ActivityResuts
{
    internal class ActivityValidationManager : IResultsManager, IDisposable
    {
        private readonly IInputGenerator _inputGenerator;
        private readonly ActivityHistoryRepository _activityRepository;
        private ActivityContext _activityContext;

        private ActivityHistory _savedActivityHistoryEntry;

        public ActivityValidationManager(IInputGenerator inputGenerator, ActivityHistoryRepository activityRepository, ActivityContext activityContext)
        {
            _inputGenerator = inputGenerator;
            _activityRepository = activityRepository;
            _activityContext = activityContext;

            SaveState();
        }

        public void SaveState()
        {
            _savedActivityHistoryEntry = GetActivityItem(); 
        }

        private ActivityHistory GetActivityItem()
        {

            Task<IEnumerable<ActivityHistory>> getActivityItems = Task.Run(() => _activityRepository.GetCorrelated(_activityContext.CorrelationId));
            getActivityItems.Wait();

            ActivityHistory result = null;
            var dataResult = getActivityItems.Result.ToList();
            if (dataResult.Count == 1)// up to here only 1 Activity History must exist 
            {
                result = dataResult[0];
            }
            else
            {
                throw new Exception($"Unable to get ActivityHistory record for Test Case ObjectId: {_inputGenerator.TestCaseId}");
            }

            return result;
        }

        private List<ActivityHistory> GetAllActivityItems()
        {

            Task<IEnumerable<ActivityHistory>> getActivityItems = Task.Run(() => _activityRepository.GetCorrelated(_activityContext.CorrelationId));
            getActivityItems.Wait();

            return getActivityItems.Result.ToList();
        }

        public bool Validate()
        {
            string resultValidatorClassName = _inputGenerator.TestCaseCollection.GetActivityValidator(_inputGenerator.TestCaseId); 


            string objectToInstantiate = $"CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ActivityResults.{resultValidatorClassName}, CSE.Automation.Tests";

            var objectType = Type.GetType(objectToInstantiate);

            var newActivityHistoryEntry = GetAllActivityItems();

            object[] args = { _savedActivityHistoryEntry, newActivityHistoryEntry, _activityContext, _activityRepository,  _inputGenerator.TestCaseId};

            var instantiatedObject = Activator.CreateInstance(objectType, args) as IActivityResultValidator;

            return instantiatedObject.Validate();

        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
