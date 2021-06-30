using System.Collections.Generic;
using CluedIn.Core.Crawling;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CluedIn.Connector.Anaplan.Infrastructure
{
    public class AnaplanConnectorJobData : CrawlJobData
    {
        public AnaplanConnectorJobData(IDictionary<string, object> configuration)
        {
            ProviderConfig = ProviderConfiguration.CreateFromDictionary(configuration);
        }

        public ProviderConfiguration ProviderConfig { get; }
    }
}
