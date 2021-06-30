#nullable enable

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CluedIn.Connector.Anaplan.Infrastructure;
using CluedIn.Core.Connectors;
using CluedIn.Core.Data.Vocabularies;
using CluedIn.Core.Streams.Models;

namespace CluedIn.Connector.Anaplan.Storage
{
    public class InMemoryContainerStorage : IPersistentContainerStorage
    {
        private readonly ConcurrentDictionary<(Guid providerDefId, string name), Container> _containers = new();

        public Task CreateContainer(Guid providerDefinitionId, CreateContainerModel containerMetadata,
            ProviderConfiguration config)
        {
            if (_containers.ContainsKey((providerDefinitionId, containerMetadata.Name))
                && containerMetadata.ExistingContainerAction == ExistingContainerActionEnum.ThrowError)
            {
                throw new InvalidOperationException("Container already exists");
            }

            _containers[(providerDefinitionId, containerMetadata.Name)] =
                new Container(containerMetadata.Name, containerMetadata.DataTypes);

            return Task.CompletedTask;
        }

        public Task RemoveContainer(Guid providerDefinitionId, string name, ProviderConfiguration config)
        {
            _containers.TryRemove((providerDefinitionId, name), out _);

            return Task.CompletedTask;
        }

        public Task ClearContainer(Guid providerDefinitionId, string name, ProviderConfiguration config)
        {
            TryGetContainer(providerDefinitionId, name)?.Clear();

            return Task.CompletedTask;
        }

        public async Task RenameContainer(Guid providerDefinitionId, string oldName, string newName,
            ProviderConfiguration config)
        {
            var container = TryGetContainer(providerDefinitionId, oldName);
            if (container != null)
            {
                await RemoveContainer(providerDefinitionId, oldName, config);
                container.Name = newName;
                _containers[(providerDefinitionId, newName)] = container;
            }
        }

        public Task<IEnumerable<IStorageContainer>> GetContainers(Guid providerDefinitionId,
            ProviderConfiguration config)
        {
            var containers = _containers
                .Where(kv => kv.Key.providerDefId == providerDefinitionId)
                .Select(kv => kv.Value)
                .Cast<IStorageContainer>();

            return Task.FromResult(containers);
        }

        public Task StoreDataToContainer(Guid providerDefinitionId, string containerName,
            IDictionary<string, object> dictionary, ProviderConfiguration config)
        {
            TryGetContainer(providerDefinitionId, containerName)?.AddRow(dictionary);

            return Task.CompletedTask;
        }

#pragma warning disable 1998 // async without awaits. Make async for convenience.
        public async IAsyncEnumerable<string[]> GetContainerRows(Guid providerDefinitionId, string containerName,
            ProviderConfiguration config)
#pragma warning restore 1998
        {
            var container = TryGetContainer(providerDefinitionId, containerName);
            if (container == null)
            {
                yield break;
            }

            foreach (var row in container.GetRows())
            {
                yield return row;
            }
        }

        public Task<DateTimeOffset> GetLastExportedStamp(Guid providerDefinitionId, string containerName, ProviderConfiguration config)
        {
            var container = TryGetContainer(providerDefinitionId, containerName);
            var stamp = container?.LastExportedStamp ?? DateTimeOffset.MinValue;
            return Task.FromResult(stamp);
        }

        public Task SetLastExportedStamp(DateTimeOffset stamp, Guid providerDefinitionId, string containerName,
            ProviderConfiguration config)
        {
            var container = TryGetContainer(providerDefinitionId, containerName);
            if (container != null)
            {
                container.LastExportedStamp = stamp;
            }

            return Task.CompletedTask;
        }

        private Container? TryGetContainer(Guid providerDefinitionId, string name)
        {
            _containers.TryGetValue((providerDefinitionId, name), out var container);
            return container;
        }

        private class Container : IStorageContainer
        {
            public string Name { get; set; }

            private readonly ConnectionDataType[] _columns;
            private readonly ConnectionDataType _idColumn;

            private readonly ConcurrentDictionary<Guid, string[]> _rows = new();

            public string Id
            {
                get => Name;
                set => throw new InvalidOperationException("ID cannot be modified");
            }

            public string? FullyQualifiedName
            {
                get => null;
                set => throw new InvalidOperationException("Cannot be modified");
            }

            public IReadOnlyList<ConnectionDataType> Columns => _columns;

            public DateTimeOffset LastExportedStamp { get; set; } = DateTimeOffset.MinValue;

            public Container(string name, IList<ConnectionDataType> columns)
            {
                Name = name;

                var idColumn = columns.FirstOrDefault(x => x.Name.Equals("Id", StringComparison.OrdinalIgnoreCase));
                if (idColumn == null)
                {
                    throw new InvalidOperationException("ID column not found");
                }

                if (idColumn.Type != VocabularyKeyDataType.Guid)
                {
                    throw new InvalidOperationException("ID column should be GUID");
                }

                _idColumn = idColumn;
                _columns = columns.ToArray();
            }

            public void AddRow(IDictionary<string, object?> rows)
            {
                if (_columns.Length != rows.Count)
                {
                    throw new InvalidOperationException("Row contains wrong number of columns");
                }

                var id = Guid.Parse(GetColumnValue(_idColumn));

                var rowCols = new string[_columns.Length];
                for (var i = 0; i < _columns.Length; i++)
                {
                    rowCols[i] = GetColumnValue(_columns[i]);
                }

                _rows[id] = rowCols;

                string GetColumnValue(ConnectionDataType column) => rows[column.Name]?.ToString()!;
            }

            public void Clear()
            {
                _rows.Clear();
            }

            public IEnumerable<string[]> GetRows()
            {
                return _rows.Values;
            }
        }
    }
}
