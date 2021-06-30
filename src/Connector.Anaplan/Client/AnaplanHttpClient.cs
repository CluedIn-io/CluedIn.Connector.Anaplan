using System;
using System.Threading.Tasks;
using CluedIn.Connector.Anaplan.Infrastructure;
using RestSharp;

namespace CluedIn.Connector.Anaplan.Client
{
    public class AnaplanHttpClient : IAnaplanClient
    {
        private readonly IRestClient _restClient;
        private readonly ProviderConfiguration _config;

        public AnaplanHttpClient(IRestClient restClient, ProviderConfiguration providerConfiguration)
        {
            _restClient = restClient;
            _config = providerConfiguration;

            restClient.BaseUrl = AnaplanConstants.AnaplanApiEndpoint;
            restClient.AddDefaultHeader("Authorization", $"AnaplanAuthToken {_config.Token}");
        }

        public int MaxChunkSize => 45 * 1024 * 1024; /* Limit is 50 MB, but keep a buffer */

        public async Task VerifyConfiguration()
        {
            // Make a request to get list of import files to verify parameters.
            var request = new RestRequest($"workspaces/{_config.WorkspaceId}/models/{_config.ModelId}/files");

            await _restClient.ExecuteAsync(request, Method.GET);
        }

        public async Task StartFileUploadSession()
        {
            var request = new RestRequest($"workspaces/{_config.WorkspaceId}/models/{_config.ModelId}/files/{_config.FileId}");
            request.AddJsonBody(new StartFileUploadSessionRequest(-1));

            await _restClient.ExecuteAsync(request, Method.POST);
        }

        public async Task PutChunkToFile(int chunkId, Memory<byte> data)
        {
            var request = new RestRequest(
                $"workspaces/{_config.WorkspaceId}/models/{_config.ModelId}/files/{_config.FileId}/chunks/{chunkId}");
            request.AddFileBytes("dummy.txt", data.ToArray(), "dummy.txt", "application/octet-stream");

            await _restClient.ExecuteAsync(request, Method.PUT);
        }

        public async Task MarkFileUploadSessionAsCompleted(int chunkCount)
        {
            var request = new RestRequest($"workspaces/{_config.WorkspaceId}/models/{_config.ModelId}/files/{_config.FileId}/complete");
            request.AddJsonBody(new MarkFileUploadSessionAsCompletedRequest(chunkCount));

            await _restClient.ExecuteAsync(request, Method.POST);
        }

        private record StartFileUploadSessionRequest(int chunkCount);

        private record MarkFileUploadSessionAsCompletedRequest(int chunkCount);
    }
}
