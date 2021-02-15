using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using CSE.Automation.Model;

namespace CSE.Automation.Tests.TestDataGenerators
{
    public class EvaluateServicePrincipalFailTestData : IEnumerable<object[]>
    {

        public IEnumerator<object[]> GetEnumerator()
        {
            // No LKG
            yield return new object[]
            {
                new ServicePrincipalTestData
                {
                    ExpectedAuditCodes = new AuditCode[] { AuditCode.AttributeValidation, AuditCode.MissingOwners },
                    ExpectedUpdateMessage = null,
                    Model = new ServicePrincipalModel()
                    {
                        AppDisplayName = "Fail1 - No LKG, No Owners",
                        AppId = "AppId1",
                        Created = DateTimeOffset.Now.AddDays(-1),
                        Deleted = null,
                        DisplayName = "Display Name1",
                        Id = "NO_LKG1",
                        LastUpdated = null,
                        Notes = null,
                        Owners = new List<string>()
                    },
                }

            };
            yield return new object[]
            {
                new ServicePrincipalTestData
                {
                    ExpectedAuditCodes = new AuditCode[] { AuditCode.AttributeValidation },
                    ExpectedUpdateMessage = new ServicePrincipalUpdateCommand
                    {
                        Action = ServicePrincipalUpdateAction.Update,
                        Notes = (null, "user1@mydirectory.com")
                    },
                    Model = new ServicePrincipalModel
                    {
                        AppDisplayName = "Fail2 - No LKG, One Owner",
                        AppId = "AppId2",
                        Created = DateTimeOffset.Now.AddDays(-1),
                        Deleted = null,
                        DisplayName = "Display Name2",
                        Id = "NO_LKG2",
                        LastUpdated = null,
                        Owners = new List<string>() { "user1@mydirectory.com" }
                    },
                }
            };
            yield return new object[]
            {
                new ServicePrincipalTestData
                {
                    ExpectedAuditCodes = new AuditCode[] { AuditCode.AttributeValidation },
                    ExpectedUpdateMessage = new ServicePrincipalUpdateCommand
                    {
                        Action = ServicePrincipalUpdateAction.Update,
                        Notes = (null, "user1@mydirectory.com, user2@mydirectory.com")
                    },
                    Model = new ServicePrincipalModel()
                    {
                        AppDisplayName = "Fail3 - No LKG, Two Owners",
                        AppId = "AppId3",
                        Created = DateTimeOffset.Now.AddDays(-1),
                        Deleted = null,
                        DisplayName = "Display Name2",
                        Id = "NO_LKG3",
                        LastUpdated = null,
                        Owners = new List<string>() {"user1@mydirectory.com, user2@mydirectory.com"}
                    },
                },
            };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

}
