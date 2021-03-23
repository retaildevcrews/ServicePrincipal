using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using CSE.Automation.Model;

namespace CSE.Automation.Tests.TestDataGenerators
{
    public class EvaluateServicePrincipalPassTestDataGenerator : IEnumerable<object[]>
    {

        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[]
            {
                new ServicePrincipalEvaluateTestData
                {
                    ExpectedAuditCodes = new AuditCode[] { AuditCode.Pass },
                    ExpectedUpdateMessage = null,
                    Target = new ServicePrincipalModel()
                    {
                        AppDisplayName = "Valid1 - No LKG, Single Owner",
                        AppId = "AppId1",
                        Created = DateTimeOffset.Now.AddDays(-1),
                        Deleted = null,
                        DisplayName = "Display Name1",
                        Id = "Id1",
                        LastUpdated = null,
                        BusinessOwners = "user1@mydirectory.com"
                    },
                }

            };
            yield return new object[]
            {
                new ServicePrincipalEvaluateTestData
                {
                    ExpectedAuditCodes = new AuditCode[] { AuditCode.Pass },
                    ExpectedUpdateMessage = null,
                    Target = new ServicePrincipalModel()
                    {
                        AppDisplayName = "Valid2 - No LKG, Two Owners, commas",
                        AppId = "AppId2",
                        Created = DateTimeOffset.Now.AddDays(-1),
                        Deleted = null,
                        DisplayName = "Display Name2",
                        Id = "Id2",
                        LastUpdated = null,
                        BusinessOwners = "user1@mydirectory.com, user2@mydirectory.com"
                    },
                }
            };
            yield return new object[]
            {
                new ServicePrincipalEvaluateTestData
                {
                    ExpectedAuditCodes = new AuditCode[] { AuditCode.Pass },
                    ExpectedUpdateMessage = null,
                    Target = new ServicePrincipalModel()
                    {
                        AppDisplayName = "Valid3 - No LKG, Two Owners, semicolon",
                        AppId = "AppId3",
                        Created = DateTimeOffset.Now.AddDays(-1),
                        Deleted = null,
                        DisplayName = "Display Name3",
                        Id = "Id3",
                        LastUpdated = null,
                        BusinessOwners = "user1@mydirectory.com;    user2@mydirectory.com"
                    },
                }
            };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
