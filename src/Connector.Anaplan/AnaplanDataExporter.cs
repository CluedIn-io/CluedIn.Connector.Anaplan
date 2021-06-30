using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using CluedIn.Connector.Anaplan.Client;
using CluedIn.Connector.Anaplan.Infrastructure;
using CluedIn.Connector.Anaplan.Storage;
using CluedIn.Core;
using CluedIn.Core.Data.Relational;
using CluedIn.Core.DataStore;
using CsvHelper;
using CsvHelper.Configuration;

namespace CluedIn.Connector.Anaplan
{
    public class AnaplanDataExporter
    {
        private readonly ConcurrentDictionary<(Guid providerDefinitionId, string containerName), byte> _outgoingExports = new();

        public async Task ExportAllProviders(ApplicationContext appContext, TimeSpan exportInterval)
        {
            var configStore = appContext.Container.Resolve<IConfigurationRepository>();
            var containerStorage = appContext.Container.Resolve<IPersistentContainerStorage>();

            using var systemExecCtx = appContext.System.CreateExecutionContext();

            var organizations = await appContext.System.Organization.DataStores
                .GetDataStore<OrganizationProfile>()
                .SelectAsync(systemExecCtx, _ => true);

            foreach (var org in organizations)
            {
                using var orgExecCtx = appContext.CreateExecutionContext(org.Id);

                foreach (var providerDef in orgExecCtx.Organization.Providers.ProviderDefinitions)
                {
                    if (providerDef.ProviderId == AnaplanConstants.ProviderId)
                    {
                        var configuration = configStore.GetConfigurationById(orgExecCtx, providerDef.Id);
                        var providerConfig = ProviderConfiguration.CreateFromDictionary(configuration);

                        var containers = await containerStorage.GetContainers(providerDef.Id, providerConfig);
                        foreach (var container in containers)
                        {
                            var lastExportStamp =
                                await containerStorage.GetLastExportedStamp(providerDef.Id, container.Name, providerConfig);

                            var currentStamp = DateTimeOffset.UtcNow;
                            if (currentStamp - lastExportStamp < exportInterval)
                            {
                                continue;
                            }

                            // Verify that current export is not being already run
                            if (!_outgoingExports.TryAdd((providerDef.Id, container.Name), 0))
                            {
                                continue;
                            }

                            var execCtxCopy = orgExecCtx.Clone();
                            _ = Task.Run(async () =>
                            {
                                await ExportContainer(container, providerConfig, providerDef.Id, execCtxCopy);
                                await containerStorage.SetLastExportedStamp(currentStamp, providerDef.Id, container.Name, providerConfig);

                                _outgoingExports.TryRemove((providerDef.Id, container.Name), out _);
                            });
                        }
                    }
                }
            }
        }

        private async Task ExportContainer(IStorageContainer container, ProviderConfiguration providerConfig, Guid providerDefId,
            ExecutionContext execCtxCopy)
        {
            // TODO
            // Enhance implementation to not accumulate the whole data in memory,
            // but instead flush chunks as longs as they are filled.

            var containerStorage = execCtxCopy.ApplicationContext.Container.Resolve<IPersistentContainerStorage>();
            var csvBytes = await BuildCsvDumpOfData(container, containerStorage, providerDefId, providerConfig);

            var anaplanClient = execCtxCopy.ApplicationContext.Container.Resolve<IAnaplanClientFactory>().Create(providerConfig);
            await WriteCsvToAnaplan(csvBytes, anaplanClient);
        }

        private static async Task<byte[]> BuildCsvDumpOfData(IStorageContainer container, IPersistentContainerStorage containerStorage,
            Guid providerDefId, ProviderConfiguration config)
        {
            await using var memStream = new MemoryStream();
            var writer = new CsvWriter(new StreamWriter(memStream), new CsvConfiguration(CultureInfo.InvariantCulture));

            // Write header
            foreach (var col in container.Columns)
            {
                writer.WriteField(col.Name);
            }

            await writer.NextRecordAsync();

            await foreach (var row in containerStorage.GetContainerRows(providerDefId, container.Name, config))
            {
                foreach (var col in row)
                {
                    writer.WriteField(col);
                }

                await writer.NextRecordAsync();
            }

            await writer.FlushAsync();
            await writer.Context.Writer.FlushAsync();

            return memStream.ToArray();
        }

        private async Task WriteCsvToAnaplan(byte[] csvBytes, IAnaplanClient anaplanClient)
        {
            await anaplanClient.StartFileUploadSession();

            var chunkSize = anaplanClient.MaxChunkSize;
            int offset = 0;
            int chunkId = -1;
            while (csvBytes.Length > offset)
            {
                var remainingBytes = csvBytes.Length - offset;
                var nextChunk = csvBytes.AsMemory().Slice(offset, Math.Min(anaplanClient.MaxChunkSize, remainingBytes));

                await anaplanClient.PutChunkToFile(++chunkId, nextChunk);
                offset += nextChunk.Length;
            }

            await anaplanClient.MarkFileUploadSessionAsCompleted(chunkId + 1);
        }
    }
}
