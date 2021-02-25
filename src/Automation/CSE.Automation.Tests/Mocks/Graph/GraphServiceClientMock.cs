using System;
using System.Linq;
using Microsoft.Graph;
using Microsoft.Graph.Core.Requests;
using NSubstitute;

namespace CSE.Automation.Tests.Mocks.Graph
{
    internal class GraphServiceClientMock : IGraphServiceClient
    {
        private readonly ServicePrincipalDeltaRequestMock requestMock = new ServicePrincipalDeltaRequestMock();
        private IGraphServiceServicePrincipalsCollectionRequestBuilder builder;

        public void WithData(ServicePrincipal[] page1, ServicePrincipal[] page2 = null)
        {
            requestMock.WithData(page1, page2);

            // Delta request Setup
            builder = Substitute.For<IGraphServiceServicePrincipalsCollectionRequestBuilder>();
            var deltaBuilder = Substitute.For<IServicePrincipalDeltaRequestBuilder>();
            deltaBuilder.Request().ReturnsForAnyArgs(requestMock);
            builder.Delta().Returns(deltaBuilder);

            // ServicePrincipal request setup
            foreach (var item in requestMock.MergedData.Values)
            {
                var spRequest = Substitute.For<IServicePrincipalRequest>();
                //spRequest.QueryOptions.Add(new QueryOption("id", item.Id));
                spRequest.Expand("").ReturnsForAnyArgs(spRequest);
                spRequest.GetAsync().Returns(requestMock[item.Id]);
                var spRequestBuilder = Substitute.For<IServicePrincipalRequestBuilder>();
                spRequestBuilder.Request().Returns(spRequest);
                builder[item.Id].Returns(spRequestBuilder);
            }
        }

        public IAuthenticationProvider AuthenticationProvider { get; }
        public string BaseUrl { get; }
        public IHttpProvider HttpProvider { get; }
        public Func<IAuthenticationProvider> PerRequestAuthProvider { get; set; }
        public IBatchRequestBuilder Batch { get; }
        public IGraphServiceInvitationsCollectionRequestBuilder Invitations { get; }
        public IGraphServiceUsersCollectionRequestBuilder Users { get; }
        public IGraphServiceIdentityProvidersCollectionRequestBuilder IdentityProviders { get; }
        public IGraphServiceApplicationsCollectionRequestBuilder Applications { get; }
        public IGraphServiceCertificateBasedAuthConfigurationCollectionRequestBuilder CertificateBasedAuthConfiguration { get; }
        public IGraphServiceContactsCollectionRequestBuilder Contacts { get; }
        public IGraphServiceContractsCollectionRequestBuilder Contracts { get; }
        public IGraphServiceDevicesCollectionRequestBuilder Devices { get; }
        public IGraphServiceDirectoryObjectsCollectionRequestBuilder DirectoryObjects { get; }
        public IGraphServiceDirectoryRolesCollectionRequestBuilder DirectoryRoles { get; }
        public IGraphServiceDirectoryRoleTemplatesCollectionRequestBuilder DirectoryRoleTemplates { get; }
        public IGraphServiceDomainDnsRecordsCollectionRequestBuilder DomainDnsRecords { get; }
        public IGraphServiceDomainsCollectionRequestBuilder Domains { get; }
        public IGraphServiceGroupsCollectionRequestBuilder Groups { get; }
        public IGraphServiceGroupSettingsCollectionRequestBuilder GroupSettings { get; }
        public IGraphServiceGroupSettingTemplatesCollectionRequestBuilder GroupSettingTemplates { get; }
        public IGraphServiceOauth2PermissionGrantsCollectionRequestBuilder Oauth2PermissionGrants { get; }
        public IGraphServiceOrganizationCollectionRequestBuilder Organization { get; }
        public IGraphServicePermissionGrantsCollectionRequestBuilder PermissionGrants { get; }
        public IGraphServiceScopedRoleMembershipsCollectionRequestBuilder ScopedRoleMemberships { get; }

        public IGraphServiceServicePrincipalsCollectionRequestBuilder ServicePrincipals { get { return builder; } }

        public IGraphServiceSubscribedSkusCollectionRequestBuilder SubscribedSkus { get; }
        public IGraphServiceWorkbooksCollectionRequestBuilder Workbooks { get; }
        public IGraphServicePlacesCollectionRequestBuilder Places { get; }
        public IGraphServiceDrivesCollectionRequestBuilder Drives { get; }
        public IGraphServiceSharesCollectionRequestBuilder Shares { get; }
        public IGraphServiceSitesCollectionRequestBuilder Sites { get; }
        public IGraphServiceSchemaExtensionsCollectionRequestBuilder SchemaExtensions { get; }
        public IGraphServiceGroupLifecyclePoliciesCollectionRequestBuilder GroupLifecyclePolicies { get; }
        public IGraphServiceDataPolicyOperationsCollectionRequestBuilder DataPolicyOperations { get; }
        public IGraphServiceSubscriptionsCollectionRequestBuilder Subscriptions { get; }
        public IGraphServiceChatsCollectionRequestBuilder Chats { get; }
        public IGraphServiceTeamsCollectionRequestBuilder Teams { get; }
        public IGraphServiceTeamsTemplatesCollectionRequestBuilder TeamsTemplates { get; }
        public IAuditLogRootRequestBuilder AuditLogs { get; }
        public IIdentityContainerRequestBuilder Identity { get; }
        public IDirectoryRequestBuilder Directory { get; }
        public IUserRequestBuilder Me { get; }
        public IPolicyRootRequestBuilder Policies { get; }
        public IEducationRootRequestBuilder Education { get; }
        public IDriveRequestBuilder Drive { get; }
        public ICloudCommunicationsRequestBuilder Communications { get; }
        public IDeviceAppManagementRequestBuilder DeviceAppManagement { get; }
        public IDeviceManagementRequestBuilder DeviceManagement { get; }
        public IReportRootRequestBuilder Reports { get; }
        public ISearchEntityRequestBuilder Search { get; }
        public IPlannerRequestBuilder Planner { get; }
        public ISecurityRequestBuilder Security { get; }
        public IAppCatalogsRequestBuilder AppCatalogs { get; }
        public ITeamworkRequestBuilder Teamwork { get; }
        public IInformationProtectionRequestBuilder InformationProtection { get; }
    }
}
