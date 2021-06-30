using CluedIn.Connector.Anaplan.Infrastructure;

namespace CluedIn.Connector.Anaplan.Client
{
    public interface IAnaplanClientFactory
    {
        IAnaplanClient Create(ProviderConfiguration providerConfiguration);
    }
}
