using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using CSE.Automation.Model;
using Newtonsoft.Json.Linq;

namespace CSE.Automation.Tests.TestDataGenerators
{
    public class EvaluateServicePrincipalFailTestDataGenerator : IEnumerable<object[]>
    {

        public IEnumerator<object[]> GetEnumerator()
        {
            // No LKG
            yield return new object[]
            {
                new ServicePrincipalEvaluateTestData
                {
                    ExpectedAuditCodes = new AuditCode[] { AuditCode.AttributeValidation, AuditCode.MissingOwners },
                    ExpectedUpdateMessage = null,
                    Target = new ServicePrincipalModel()
                    {
                        AppDisplayName = "Fail1 - No LKG, No Owners, Cannot Remediate",
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
                new ServicePrincipalEvaluateTestData
                {
                    ExpectedAuditCodes = new AuditCode[] { AuditCode.AttributeValidation },
                    ExpectedUpdateMessage = new ServicePrincipalUpdateCommand
                    {
                        Action = ServicePrincipalUpdateAction.Update,
                        Notes = (null, "user1@mydirectory.com")
                    },
                    Target = new ServicePrincipalModel
                    {
                        AppDisplayName = "Fail2 - No LKG, One Owner, Remediate from Owners",
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
                new ServicePrincipalEvaluateTestData
                {
                    ExpectedAuditCodes = new AuditCode[] { AuditCode.AttributeValidation },
                    ExpectedUpdateMessage = new ServicePrincipalUpdateCommand
                    {
                        Action = ServicePrincipalUpdateAction.Update,
                        Notes = (null, "user1@mydirectory.com, user2@mydirectory.com")
                    },
                    Target = new ServicePrincipalModel()
                    {
                        AppDisplayName = "Fail3 - No LKG, Two Owners, Remediate from Owners",
                        AppId = "AppId3",
                        Created = DateTimeOffset.Now.AddDays(-1),
                        Deleted = null,
                        DisplayName = "Display Name3",
                        Id = "NO_LKG3",
                        LastUpdated = null,
                        Owners = new List<string>() {"user1@mydirectory.com, user2@mydirectory.com"}
                    },
                },
            };
            yield return new object[]
            {
                new ServicePrincipalEvaluateTestData
                {
                    ExpectedAuditCodes = new AuditCode[] { AuditCode.InvalidDirectoryUPN },
                    ExpectedUpdateMessage = new ServicePrincipalUpdateCommand
                    {
                        Action = ServicePrincipalUpdateAction.Update,
                        Notes = ("user3@mydirectory.com", "user1@mydirectory.com, user2@mydirectory.com")
                    },
                    Target = new ServicePrincipalModel()
                    {
                        AppDisplayName = "Fail4 - No LKG, Bad UPN in notes (single), Remediate from Owners",
                        AppId = "AppId4",
                        Created = DateTimeOffset.Now.AddDays(-1),
                        Deleted = null,
                        DisplayName = "Display Name4",
                        Id = "NO_LKG4",
                        LastUpdated = null,
                        Notes = "user3@mydirectory.com",
                        Owners = new List<string>() {"user1@mydirectory.com, user2@mydirectory.com"}
                    },
                },
            };
            yield return new object[]
            {
                new ServicePrincipalEvaluateTestData
                {
                    ExpectedAuditCodes = new AuditCode[] { AuditCode.InvalidDirectoryUPN },
                    ExpectedUpdateMessage = new ServicePrincipalUpdateCommand
                    {
                        Action = ServicePrincipalUpdateAction.Update,
                        Notes = ("user3@mydirectory.com, user2@mydirectory.com", "user1@mydirectory.com, user2@mydirectory.com")
                    },
                    Target = new ServicePrincipalModel()
                    {
                        AppDisplayName = "Fail5 - No LKG, Bad UPN in notes (embedded), Remediate from Owners",
                        AppId = "AppId5",
                        Created = DateTimeOffset.Now.AddDays(-1),
                        Deleted = null,
                        DisplayName = "Display Name5",
                        Id = "NO_LKG5",
                        LastUpdated = null,
                        Notes = "user3@mydirectory.com, user2@mydirectory.com",
                        Owners = new List<string>() {"user1@mydirectory.com, user2@mydirectory.com"}
                    },
                },
            };
            yield return new object[]
            {
                new ServicePrincipalEvaluateTestData
                {
                    ExpectedAuditCodes = new AuditCode[] { AuditCode.AttributeValidation, AuditCode.InvalidDirectoryUPN },
                    ExpectedUpdateMessage = new ServicePrincipalUpdateCommand
                    {
                        Action = ServicePrincipalUpdateAction.Update,
                        Notes = ("Please call Joe.Smith@gmail.com for information", "user1@mydirectory.com, user2@mydirectory.com")
                    },
                    Target = new ServicePrincipalModel()
                    {
                        AppDisplayName = "Fail6 - No LKG, Random message in Notes, Remediate from Owners",
                        AppId = "AppId6",
                        Created = DateTimeOffset.Now.AddDays(-1),
                        Deleted = null,
                        DisplayName = "Display Name6",
                        Id = "NO_LKG6",
                        LastUpdated = null,
                        Notes = "Please call Joe.Smith@gmail.com for information",
                        Owners = new List<string>() {"user1@mydirectory.com, user2@mydirectory.com"}
                    },
                },
            };
            yield return new object[]
            {
                new ServicePrincipalEvaluateTestData
                {
                    ExpectedAuditCodes = new AuditCode[] { AuditCode.InvalidDirectoryUPN },
                    ExpectedUpdateMessage = new ServicePrincipalUpdateCommand
                    {
                        Action = ServicePrincipalUpdateAction.Update,
                        Notes = ("Joe.Smith@gmail.com", "user1@mydirectory.com, user2@mydirectory.com")
                    },
                    Target = new ServicePrincipalModel()
                    {
                        AppDisplayName = "Fail7 - No LKG, Custodian not in Directory, Remediate from Owners",
                        AppId = "AppId6",
                        Created = DateTimeOffset.Now.AddDays(-1),
                        Deleted = null,
                        DisplayName = "Display Name7",
                        Id = "NO_LKG7",
                        LastUpdated = null,
                        Notes = "Joe.Smith@gmail.com",
                        Owners = new List<string>() {"user1@mydirectory.com, user2@mydirectory.com"}
                    },
                },
            };

            // LKG
            yield return new object[]
            {
                new ServicePrincipalEvaluateTestData
                {
                    ObjectServiceData = new TrackingModel[]
                    {
                        new TrackingModel() { Id = "LKG1", Entity = JObject.FromObject(new ServicePrincipalModel() { Id = "LKG1", Notes = "LKG1@mydirectory.com" }) }
                    },
                    ExpectedAuditCodes = new AuditCode[] { AuditCode.AttributeValidation },
                    ExpectedUpdateMessage = new ServicePrincipalUpdateCommand
                    {
                        Action = ServicePrincipalUpdateAction.Revert,
                        Notes = (null, "LKG1@mydirectory.com" )
                    },
                    Target = new ServicePrincipalModel()
                    {
                        AppDisplayName = "Fail_LKG1 - LKG, No Owners, Remediate from LKG",
                        AppId = "AppId1",
                        Created = DateTimeOffset.Now.AddDays(-1),
                        Deleted = null,
                        DisplayName = "Display Name1",
                        Id = "LKG1",
                        LastUpdated = null,
                        Notes = null,
                        Owners = new List<string>()
                    },
                }

            };
            yield return new object[]
            {
                new ServicePrincipalEvaluateTestData
                {
                    ObjectServiceData = new TrackingModel[]
                    {
                        new TrackingModel() { Id = "LKG2", Entity = JObject.FromObject(new ServicePrincipalModel() { Id = "LKG2", Notes = "LKG2@mydirectory.com" }) }
                    },
                    ExpectedAuditCodes = new AuditCode[] { AuditCode.AttributeValidation },
                    ExpectedUpdateMessage = new ServicePrincipalUpdateCommand
                    {
                        Action = ServicePrincipalUpdateAction.Revert,
                        Notes = (null, "LKG2@mydirectory.com" )
                    },
                    Target = new ServicePrincipalModel()
                    {
                        AppDisplayName = "Fail_LKG2 - LKG, With Owners, Remediate from LKG",
                        AppId = "AppId2",
                        Created = DateTimeOffset.Now.AddDays(-1),
                        Deleted = null,
                        DisplayName = "Display Name2",
                        Id = "LKG2",
                        LastUpdated = null,
                        Notes = null,
                        Owners = new List<string>() { "user1@mydirectory.com" }
                    },
                }

            };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

}
