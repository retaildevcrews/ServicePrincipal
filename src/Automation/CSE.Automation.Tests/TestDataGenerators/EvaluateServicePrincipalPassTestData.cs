using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using CSE.Automation.Model;

namespace CSE.Automation.Tests.TestDataGenerators
{
   public  class EvaluateServicePrincipalPassTestData : IEnumerable<object[]>
    {

        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[]
            {
                new ServicePrincipalModel()
                {
                    AppDisplayName = "Valid1 - No LKG, Single Owner",
                    AppId = "AppId1",
                    Created = DateTimeOffset.Now.AddDays(-1),
                    Deleted = null,
                    DisplayName = "Display Name1",
                    Id = "Id1",
                    LastUpdated = null,
                    Notes = "user1@mydirectory.com"
                }
            };
            yield return new object[]
            {
                new ServicePrincipalModel()
                {
                    AppDisplayName = "Valid2 - No LKG, Two Owners",
                    AppId = "AppId2",
                    Created = DateTimeOffset.Now.AddDays(-1),
                    Deleted = null,
                    DisplayName = "Display Name2",
                    Id = "Id2",
                    LastUpdated = null,
                    Notes = "user1@mydirectory.com, user2@mydirectory.com"
                }
            };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator(); 
    }
}
