using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CluedIn.Core;
using CluedIn.Core.Crawling;
using CluedIn.Core.Data.Relational;
using CluedIn.Core.Providers;
using CluedIn.Core.Webhooks;
using CluedIn.Providers.Models;

namespace CluedIn.Connector.Anaplan.Infrastructure
{
    public class AnaplanConnectorProvider : ProviderBase, IExtendedProviderMetadata
    {
        public AnaplanConnectorProvider(ApplicationContext appContext) : base(appContext, AnaplanConstants.CreateProviderMetadata())
        {
        }

        public override Task<CrawlJobData> GetCrawlJobData(ProviderUpdateContext context,
            IDictionary<string, object> configuration, Guid organizationId, Guid userId,
            Guid providerDefinitionId)
        {
            var jobData = new AnaplanConnectorJobData(configuration);
            return Task.FromResult<CrawlJobData>(jobData);
        }

        public override Task<bool> TestAuthentication(ProviderUpdateContext context, IDictionary<string, object> configuration,
            Guid organizationId, Guid userId,
            Guid providerDefinitionId)
        {
            throw new NotImplementedException();
        }

        public override Task<ExpectedStatistics> FetchUnSyncedEntityStatistics(ExecutionContext context,
            IDictionary<string, object> configuration, Guid organizationId,
            Guid userId, Guid providerDefinitionId)
        {
            throw new NotImplementedException();
        }

        public override Task<IDictionary<string, object>> GetHelperConfiguration(ProviderUpdateContext context, CrawlJobData jobData,
            Guid organizationId, Guid userId,
            Guid providerDefinitionId)
        {
            var dict = ((AnaplanConnectorJobData)jobData).ProviderConfig.ToDictionary();
            return Task.FromResult(dict);
        }

        public override Task<IDictionary<string, object>> GetHelperConfiguration(ProviderUpdateContext context,
            CrawlJobData jobData, Guid organizationId, Guid userId, Guid providerDefinitionId, string folderId) =>
            GetHelperConfiguration(context, jobData, organizationId, userId, providerDefinitionId);

        public override Task<AccountInformation> GetAccountInformation(ExecutionContext context, CrawlJobData jobData, Guid organizationId,
            Guid userId,
            Guid providerDefinitionId)
        {
            var anaplanJobData = (AnaplanConnectorJobData)jobData;

            var accountInfo = new AccountInformation(
                anaplanJobData.ProviderConfig.Token,
                $"WKS: {anaplanJobData.ProviderConfig.WorkspaceId}, FILE: {anaplanJobData.ProviderConfig.FileId}");
            return Task.FromResult(accountInfo);
        }

        public override string Schedule(DateTimeOffset relativeDateTime, bool webHooksEnabled)
        {
            throw new NotImplementedException();
        }

        public override Task<IEnumerable<WebHookSignature>> CreateWebHook(ExecutionContext context, CrawlJobData jobData,
            IWebhookDefinition webhookDefinition,
            IDictionary<string, object> config)
        {
            throw new NotImplementedException();
        }

        public override Task<IEnumerable<WebhookDefinition>> GetWebHooks(ExecutionContext context)
        {
            throw new NotImplementedException();
        }

        public override Task DeleteWebHook(ExecutionContext context, CrawlJobData jobData, IWebhookDefinition webhookDefinition)
        {
            throw new NotImplementedException();
        }

        public override Task<CrawlLimit> GetRemainingApiAllowance(ExecutionContext context, CrawlJobData jobData, Guid organizationId,
            Guid userId,
            Guid providerDefinitionId)
        {
            return Task.FromResult(new CrawlLimit(-1, TimeSpan.Zero));
        }

        public override IEnumerable<string> WebhookManagementEndpoints(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        public string Icon => AnaplanConstants.IconResourceName;
        public string Domain => AnaplanConstants.Uri;
        public string About => AnaplanConstants.ConnectorDescription;
        public AuthMethods AuthMethods => AnaplanConstants.AuthMethods;
        public IEnumerable<Control> Properties => AnaplanConstants.Properties;

        public Guide Guide { get; } = new Guide
        {
            Details = AnaplanConstants.Details,
            Instructions = AnaplanConstants.Instructions,
            Value = new List<string> {AnaplanConstants.ConnectorDescription}
        };

        public new IntegrationType Type => AnaplanConstants.Type;
        public override bool ScheduleCrawlJobs => false;
    }
}
