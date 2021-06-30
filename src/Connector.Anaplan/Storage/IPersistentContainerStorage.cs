using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CluedIn.Connector.Anaplan.Infrastructure;
using CluedIn.Core.Connectors;

namespace CluedIn.Connector.Anaplan.Storage
{
    public interface IStorageContainer : IConnectorContainer
    {
        IReadOnlyList<ConnectionDataType> Columns { get; }
    }

    public interface IPersistentContainerStorage
    {
        Task CreateContainer(Guid providerDefinitionId, CreateContainerModel containerMetadata, ProviderConfiguration config);
        Task RemoveContainer(Guid providerDefinitionId, string name, ProviderConfiguration config);
        Task ClearContainer(Guid providerDefinitionId, string name, ProviderConfiguration config);
        Task RenameContainer(Guid providerDefinitionId, string oldName, string newName, ProviderConfiguration config);
        Task<IEnumerable<IStorageContainer>> GetContainers(Guid providerDefinitionId, ProviderConfiguration config);

        Task StoreDataToContainer(Guid providerDefinitionId, string containerName, IDictionary<string, object> dictionary,
            ProviderConfiguration config);

        IAsyncEnumerable<string[]> GetContainerRows(Guid providerDefinitionId, string containerName, ProviderConfiguration config);

        Task<DateTimeOffset> GetLastExportedStamp(Guid providerDefinitionId, string containerName, ProviderConfiguration config);
        Task SetLastExportedStamp(DateTimeOffset stamp, Guid providerDefinitionId, string containerName, ProviderConfiguration config);
    }
}
