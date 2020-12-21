﻿using System;
using System.Collections.Generic;
using System.Text;
using CSE.Automation.Model;
using CSE.Automation.TestsPrep.TestCases.ServicePrincipals;
using Microsoft.Graph;
using Newtonsoft.Json;
using static CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.TestCases.TestCaseCollection;

namespace CSE.Automation.Tests.FunctionsUnitTests.TestCaseValidators.ServicePrincipalResults
{
    internal class UpdateSpResultValidator : SpResultValidatorBase, ISpResultValidator
    {

        public UpdateSpResultValidator(string savedServicePrincipalAsString, IInputGenerator inputGenerator, ActivityContext activityContext) 
                                    : base(savedServicePrincipalAsString, inputGenerator, activityContext)
        {
        }

        public override bool Validate()
        {
            Dictionary<string,string> ownersList = GraphHelper.GetOwnersDisplayNameAndUserPrincipalNameKeyValuePair(NewServicePrincipal);
            if (ownersList.Count > 0 && !string.IsNullOrEmpty(NewServicePrincipal.Notes))
            {
                foreach (var ownerName in ownersList.Values)
                {
                    if (!NewServicePrincipal.Notes.Contains(ownerName))
                    {
                        return false;
                    }
                }

                return true;
            }
            return false;
        }
    }
}