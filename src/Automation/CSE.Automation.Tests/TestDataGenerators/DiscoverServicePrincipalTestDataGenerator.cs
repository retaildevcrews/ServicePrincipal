using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSE.Automation.Model;
using CSE.Automation.Model.Commands;
using Microsoft.Graph;
using Newtonsoft.Json.Linq;

namespace CSE.Automation.Tests.TestDataGenerators
{
    public class DiscoverServicePrincipalTestDataGenerator : IEnumerable<object[]>
    {
        private readonly ServicePrincipalDiscoverTestData[] data =
        {
            
            // Zero pages of data: expect that no SPs are considered, no Evaluate messages are produced, nothing updated in LKG
            new ServicePrincipalDiscoverTestData
            {
                Target = new RequestDiscoveryCommand() { CorrelationId = Guid.NewGuid().ToString(), DiscoveryMode = DiscoveryMode.FullSeed, Source = "TEST - No Data" },
                InitialServicePrincipals1 = new ServicePrincipal[0],
                InitialObjectServiceData = new TrackingModel[0],
                ExpectedObjectServiceData = new TrackingModel[0],
                ExpectedAuditCodes = new AuditCode[0],
                ExpectedEvaluateMessages = 0,
            },

            // One page of data, no SPs in LKG: expect that one SP is considered, one Evaluate message is produced, nothing updated in LKG
            new ServicePrincipalDiscoverTestData
            {
                Target = new RequestDiscoveryCommand() { CorrelationId = Guid.NewGuid().ToString(), DiscoveryMode = DiscoveryMode.FullSeed, Source = "TEST - One Page, One SP" },
                InitialServicePrincipals1 = new ServicePrincipal[]
                {
                    new ServicePrincipal() { Id = Guid.NewGuid().ToString(), DisplayName = "SP1", AppDisplayName = "App1", AdditionalData = new Dictionary<string, object>() },
                },
                InitialObjectServiceData = new TrackingModel[0],
                ExpectedObjectServiceData = new TrackingModel[0],
                ExpectedAuditCodes = new AuditCode[0],
                ExpectedEvaluateMessages = 1,
            },

            // Multiple pages, at least one SP per page is deleted, no SPs in LKG: expect that one SP is considered, one evaluate message is produced, nothing updated in LKG
            new ServicePrincipalDiscoverTestData
            {
                Target = new RequestDiscoveryCommand() { CorrelationId = Guid.NewGuid().ToString(), DiscoveryMode = DiscoveryMode.FullSeed, Source = "TEST - One Page, One SP" },
                InitialServicePrincipals1 = new ServicePrincipal[]
                {
                    new ServicePrincipal() { Id = Guid.NewGuid().ToString(), DisplayName = "SP1", AppDisplayName = "App1", AdditionalData = new Dictionary<string, object>() },
                    new ServicePrincipal() { Id = Guid.NewGuid().ToString(), DisplayName = "SP2", AppDisplayName = "App2", AdditionalData = new Dictionary<string, object>() { {"@removed", ""} } },
                },
                InitialServicePrincipals2 = new ServicePrincipal[]
                {
                    new ServicePrincipal() { Id = Guid.NewGuid().ToString(), DisplayName = "SP3", AppDisplayName = "App3", AdditionalData = new Dictionary<string, object>() { {"@removed", ""} } },
                    new ServicePrincipal() { Id = Guid.NewGuid().ToString(), DisplayName = "SP4", AppDisplayName = "App4", AdditionalData = new Dictionary<string, object>() { {"@removed", ""} } },
                },
                InitialObjectServiceData = new TrackingModel[0],
                ExpectedObjectServiceData = new TrackingModel[0],
                ExpectedAuditCodes = new AuditCode[] { AuditCode.Deleted, AuditCode.Deleted, AuditCode.Deleted },
                ExpectedEvaluateMessages = 1,
            },
            
            // Multiple Pages, All pages have deleted SPs, All SPs are in LKG: expect that no Evaluate messages are produced, no SPs are considered and all SPs in LKG are marked as untracked
            new ServicePrincipalDiscoverTestData
            {
                Target = new RequestDiscoveryCommand() { CorrelationId = "TC4", DiscoveryMode = DiscoveryMode.FullSeed, Source = "TEST - One Page, One SP" },
                InitialServicePrincipals1 = new ServicePrincipal[]
                {
                    new ServicePrincipal() { Id = "SP1", DisplayName = "SP1", AppDisplayName = "App1", AdditionalData = new Dictionary<string, object>() { {"@removed", ""} } },
                    new ServicePrincipal() { Id = "SP2", DisplayName = "SP2", AppDisplayName = "App2", AdditionalData = new Dictionary<string, object>() { {"@removed", ""} } },
                },
                InitialServicePrincipals2 = new ServicePrincipal[]
                {
                    new ServicePrincipal() { Id = "SP3", DisplayName = "SP3", AppDisplayName = "App3", AdditionalData = new Dictionary<string, object>() { {"@removed", ""} } },
                    new ServicePrincipal() { Id = "SP4", DisplayName = "SP4", AppDisplayName = "App4", AdditionalData = new Dictionary<string, object>() { {"@removed", ""} } },
                },
                InitialObjectServiceData = new TrackingModel[]
                {
                    new TrackingModel() { Id = "SP1", CorrelationId = "TC0", Created = DateTimeOffset.Now.AddDays(-7),  State = TrackingState.Tracked, Deleted = null, Entity = JObject.FromObject(new ServicePrincipalModel() { Id = "SP1" }) },
                    new TrackingModel() { Id = "SP2", CorrelationId = "TC0", Created = DateTimeOffset.Now.AddDays(-14), State = TrackingState.Tracked, Deleted = null, Entity = JObject.FromObject(new ServicePrincipalModel() { Id = "SP2" }) },
                    new TrackingModel() { Id = "SP3", CorrelationId = "TC0", Created = DateTimeOffset.Now.AddDays(-21), State = TrackingState.Tracked, Deleted = null, Entity = JObject.FromObject(new ServicePrincipalModel() { Id = "SP3" }) },
                    new TrackingModel() { Id = "SP4", CorrelationId = "TC0", Created = DateTimeOffset.Now.AddDays(-28), State = TrackingState.Tracked, Deleted = null, Entity = JObject.FromObject(new ServicePrincipalModel() { Id = "SP4" }) },
                },
                ExpectedObjectServiceData = new TrackingModel[]
                {
                    new TrackingModel() { Id = "SP1", CorrelationId = "TC4", Created = DateTimeOffset.Now.AddDays(-7),  State = TrackingState.Untracked, Deleted = DateTimeOffset.Now, Entity = JObject.FromObject(new ServicePrincipalModel() { Id = "SP1" }) },
                    new TrackingModel() { Id = "SP2", CorrelationId = "TC4", Created = DateTimeOffset.Now.AddDays(-14), State = TrackingState.Untracked, Deleted = DateTimeOffset.Now, Entity = JObject.FromObject(new ServicePrincipalModel() { Id = "SP2" }) },
                    new TrackingModel() { Id = "SP3", CorrelationId = "TC4", Created = DateTimeOffset.Now.AddDays(-21), State = TrackingState.Untracked, Deleted = DateTimeOffset.Now, Entity = JObject.FromObject(new ServicePrincipalModel() { Id = "SP3" }) },
                    new TrackingModel() { Id = "SP4", CorrelationId = "TC4", Created = DateTimeOffset.Now.AddDays(-28), State = TrackingState.Untracked, Deleted = DateTimeOffset.Now, Entity = JObject.FromObject(new ServicePrincipalModel() { Id = "SP4" }) },
                },
                ExpectedAuditCodes = new AuditCode[] { AuditCode.Deleted, AuditCode.Deleted, AuditCode.Deleted, AuditCode.Deleted },
                ExpectedEvaluateMessages = 0,
            },

        };


        public IEnumerator<object[]> GetEnumerator()
        {
            return data.Select(item => new object[] { item }).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
