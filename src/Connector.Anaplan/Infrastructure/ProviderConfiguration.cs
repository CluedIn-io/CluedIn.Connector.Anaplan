using System;
using System.Collections.Generic;

namespace CluedIn.Connector.Anaplan.Infrastructure
{
    public class ProviderConfiguration
    {
        public string Token { get; init; }

        public string WorkspaceId { get; init; }

        public string ModelId { get; init; }

        public string FileId { get; init; }

        public string ImportId { get; init; }

        private ProviderConfiguration()
        {
        }

        public IDictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                {AnaplanConstants.KeyName.Token, Token},
                {AnaplanConstants.KeyName.WorkspaceId, WorkspaceId},
                {AnaplanConstants.KeyName.ModelId, ModelId},
                {AnaplanConstants.KeyName.ImportId, ImportId},
                {AnaplanConstants.KeyName.FileId, FileId},
            };
        }

        public static ProviderConfiguration CreateFromDictionary(IDictionary<string, object> config)
        {
            return new ProviderConfiguration
            {
                Token = GetString(AnaplanConstants.KeyName.Token),
                WorkspaceId = GetString(AnaplanConstants.KeyName.WorkspaceId),
                ModelId = GetString(AnaplanConstants.KeyName.ModelId),
                ImportId = GetString(AnaplanConstants.KeyName.ImportId),
                FileId = GetString(AnaplanConstants.KeyName.FileId)
            };

            string GetString(string key)
            {
                return config.TryGetValue(key, out var v) && v is string strValue
                    ? strValue
                    : throw new InvalidOperationException($"Required key '{key}' is not found in config");
            }
        }
    }
}
