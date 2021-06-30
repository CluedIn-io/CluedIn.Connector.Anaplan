using System;
using System.Collections.Generic;
using System.Linq;
using CluedIn.Core.Net.Mail;
using CluedIn.Core.Providers;

namespace CluedIn.Connector.Anaplan.Infrastructure
{
    internal class AnaplanConstants
    {
        public class KeyName
        {
            public const string Token = "token";
            public const string WorkspaceId = "workspaceId";
            public const string ModelId = "modelId";
            public const string ImportId = "importId";
            public const string FileId = "fileId";
        }

        public const string ConnectorName = "AnaplanConnector";
        public const string ConnectorComponentName = "AnaplanConnector";
        public const string ConnectorDescription = "Supports publishing of data to Anaplan.";
        public const string Uri = "https://www.anaplan.com/";
        public static readonly Uri AnaplanApiEndpoint = new Uri("https://api.anaplan.com/2/0/", UriKind.Absolute);

        public static readonly Guid ProviderId = Guid.Parse("{CDF61B54-358E-4C65-AA36-4CC11F626E0A}");
        public const string ProviderName = "Anaplan Connector";
        public const bool SupportsConfiguration = false;
        public const bool SupportsWebHooks = false;
        public const bool SupportsAutomaticWebhookCreation = false;
        public const bool RequiresAppInstall = false;
        public const string AppInstallUrl = null;
        public const string ReAuthEndpoint = null;

        public const string IconResourceName = "Resources.anaplan.png";
        public const string Instructions = "Provide authentication instructions here, if applicable";
        public const IntegrationType Type = IntegrationType.Connector;
        public const string Details = "Provides connectivity to a Microsoft Sql Server database";

        public static AuthMethods AuthMethods = new AuthMethods
        {
            token = new Control[]
            {
                new Control
                {
                    name = KeyName.Token,
                    displayName = "API Token",
                    type = "input",
                    isRequired = true
                },
                new Control
                {
                    name = KeyName.WorkspaceId,
                    displayName = "Workspace ID",
                    type = "input",
                    isRequired = true
                },
                new Control
                {
                    name = KeyName.ModelId,
                    displayName = "Model ID",
                    type = "input",
                    isRequired = true
                },
                new Control
                {
                    name = KeyName.ImportId,
                    displayName = "Import ID",
                    type = "input",
                    isRequired = false
                },
                new Control
                {
                    name = KeyName.FileId,
                    displayName = "File ID",
                    type = "input",
                    isRequired = true
                },
            }
        };

        public static IEnumerable<Control> Properties = Enumerable.Empty<Control>();

        public static readonly ComponentEmailDetails ComponentEmailDetails = new ComponentEmailDetails {
            Features = new Dictionary<string, string>
            {
                                       { "Connectivity",        "Expenses and Invoices against customers" }
                                   },
            Icon = ProviderIconFactory.CreateConnectorUri(ProviderId),
            ProviderName = ProviderName,
            ProviderId = ProviderId,
            Webhooks = SupportsWebHooks
        };

        public static IProviderMetadata CreateProviderMetadata()
        {
            return new ProviderMetadata {
                Id = ProviderId,
                ComponentName = ConnectorName,
                Name = ProviderName,
                Type = "Connector",
                SupportsConfiguration = SupportsConfiguration,
                SupportsWebHooks = SupportsWebHooks,
                SupportsAutomaticWebhookCreation = SupportsAutomaticWebhookCreation,
                RequiresAppInstall = RequiresAppInstall,
                AppInstallUrl = AppInstallUrl,
                ReAuthEndpoint = ReAuthEndpoint,
                ComponentEmailDetails = ComponentEmailDetails
            };
        }
    }
}
