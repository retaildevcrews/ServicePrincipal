using System.Collections.Generic;
using CSE.Automation.DataAccess;
using CSE.Automation.Model;
using static CSE.Automation.Tests.UnitTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.UnitTests.TestCaseValidators.ActivityResuts
{
    abstract class ActivityResultValidatorBase : IActivityResultValidator
    {
        public TestCase TestCaseID { get; }
        public ActivityHistory SavedActivityEntry { get; }
        public List<ActivityHistory> ActivityHistoryList { get; }
        public ActivityContext Context { get; }
        public ActivityHistoryRepository Repository { get; }

        public ActivityResultValidatorBase(ActivityHistory savedActivityEntry, List<ActivityHistory> activityHistoryList, 
                                        ActivityContext activityContext, ActivityHistoryRepository activityRepository, TestCase testCase)
        {
            SavedActivityEntry = savedActivityEntry;
            ActivityHistoryList = activityHistoryList;
            TestCaseID = testCase;
            Context = activityContext;
            Repository = activityRepository;
        }

        public abstract bool Validate();
    }
}
