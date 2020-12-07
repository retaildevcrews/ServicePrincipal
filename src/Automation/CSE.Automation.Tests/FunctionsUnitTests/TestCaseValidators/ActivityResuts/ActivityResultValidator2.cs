﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSE.Automation.DataAccess;
using CSE.Automation.Model;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ActivityResults
{

    internal class ActivityResultValidator2 : ActivityResultValidatorBase, IActivityResultValidator
    {
        public ActivityResultValidator2(ActivityHistory savedActivityEntry, List<ActivityHistory> activityHistoryList, 
                                        ActivityContext activityContext, ActivityHistoryRepository activityRepository, TestCase testCase)
                                        : base(savedActivityEntry, activityHistoryList, activityContext, activityRepository, testCase)
        {
        }
        public override bool Validate()
        {
            ActivityHistory newActivityItem = ActivityHistoryList.FirstOrDefault(x => x.Name == "Delta Discovery");

            if (newActivityItem != null)
            {
                bool lastUpdatedPass = newActivityItem.LastUpdated > SavedActivityEntry.LastUpdated;

                bool correlationIdPass = newActivityItem.CorrelationId == Context.CorrelationId;

                bool sourcePass = newActivityItem.CommandSource == "HTTP";

                return (lastUpdatedPass && correlationIdPass && sourcePass);
            }
            else
            {
                return false;
            }
        }
    }
}
