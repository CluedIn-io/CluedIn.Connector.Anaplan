using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CluedIn.Connector.Anaplan.Client;
using CluedIn.Connector.Anaplan.Infrastructure;
using CluedIn.Connector.Anaplan.Storage;
using CluedIn.Core;
using CluedIn.Core.Connectors;
using CluedIn.Core.DataStore;

namespace CluedIn.Connector.Anaplan
{
    public class AnaplanConnector : ConnectorBase
    {
        private IAnaplanClientFactory ClientFactory { get; }
        public IPersistentContainerStorage ContainerStorage { get; }

        public AnaplanConnector(IConfigurationRepository configurationRepository, IAnaplanClientFactory clientFactory,
            IPersistentContainerStorage containerStorage)
            : base(configurationRepository)
        {
            ProviderId = AnaplanConstants.ProviderId;

            ClientFactory = clientFactory;
            ContainerStorage = containerStorage;
        }

        public override async Task<IEnumerable<IConnectorContainer>> GetContainers(ExecutionContext executionContext,
            Guid providerDefinitionId)
        {
            var config = await GetProviderConfig(executionContext, providerDefinitionId);
            return await ContainerStorage.GetContainers(providerDefinitionId, config);
        }

        public override Task<IEnumerable<IConnectionDataType>> GetDataTypes(ExecutionContext executionContext, Guid providerDefinitionId,
            string containerId)
        {
            throw new NotImplementedException();
        }

        public override async Task<bool> VerifyConnection(ExecutionContext executionContext, IDictionary<string, object> authenticationData)
        {
            try
            {
                var config = ProviderConfiguration.CreateFromDictionary(authenticationData);
                var client = ClientFactory.Create(config);

                await client.VerifyConfiguration();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public override async Task CreateContainer(ExecutionContext executionContext, Guid providerDefinitionId, CreateContainerModel model)
        {
            var config = await GetProviderConfig(executionContext, providerDefinitionId);
            await ContainerStorage.CreateContainer(providerDefinitionId, model, config);
        }

        public override async Task EmptyContainer(ExecutionContext executionContext, Guid providerDefinitionId, string id)
        {
            var config = await GetProviderConfig(executionContext, providerDefinitionId);
            await ContainerStorage.ClearContainer(providerDefinitionId, id, config);
        }

        public override async Task RemoveContainer(ExecutionContext executionContext, Guid providerDefinitionId, string id)
        {
            var config = await GetProviderConfig(executionContext, providerDefinitionId);
            await ContainerStorage.RemoveContainer(providerDefinitionId, id, config);
        }

        public override async Task ArchiveContainer(ExecutionContext executionContext, Guid providerDefinitionId, string id)
        {
            var config = await GetProviderConfig(executionContext, providerDefinitionId);
            await ContainerStorage.RemoveContainer(providerDefinitionId, id, config);
        }

        public override async Task RenameContainer(ExecutionContext executionContext, Guid providerDefinitionId, string id, string newName)
        {
            var config = await GetProviderConfig(executionContext, providerDefinitionId);
            await ContainerStorage.RenameContainer(providerDefinitionId, id, newName, config);
        }

        public override async Task StoreData(ExecutionContext executionContext, Guid providerDefinitionId, string containerName,
            IDictionary<string, object> data)
        {
            var config = await GetProviderConfig(executionContext, providerDefinitionId);
            await ContainerStorage.StoreDataToContainer(providerDefinitionId, containerName, data, config);
        }

        public override Task StoreEdgeData(ExecutionContext executionContext, Guid providerDefinitionId, string containerName,
            string originEntityCode, IEnumerable<string> edges)
        {
            throw new NotSupportedException("Storing edges is not supported");
        }

        private async Task<ProviderConfiguration> GetProviderConfig(ExecutionContext executionContext, Guid providerDefinitionId)
        {
            var authDetails = await GetAuthenticationDetails(executionContext, providerDefinitionId);
            return ProviderConfiguration.CreateFromDictionary(authDetails.Authentication);
        }
    }
}
