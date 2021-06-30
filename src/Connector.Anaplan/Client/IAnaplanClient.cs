using System;
using System.Threading.Tasks;

namespace CluedIn.Connector.Anaplan.Client
{
    public interface IAnaplanClient
    {
        int MaxChunkSize { get; }

        Task VerifyConfiguration();

        Task StartFileUploadSession();

        Task PutChunkToFile(int chunkId, Memory<byte> data);

        Task MarkFileUploadSessionAsCompleted(int chunkCount);
    }
}
