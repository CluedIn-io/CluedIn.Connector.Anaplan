using System;
using System.Reflection;
using System.Timers;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using CluedIn.Connector.Anaplan.Client;
using CluedIn.Connector.Anaplan.Storage;
using CluedIn.Core;
using CluedIn.Core.Providers;
using CluedIn.Core.Server;
using ComponentHost;

namespace CluedIn.Connector.Anaplan.Infrastructure
{
    [Component(AnaplanConstants.ConnectorComponentName, "Providers", ComponentType.Service,
        Components.Server, Components.DataStores, Isolation = ComponentIsolation.NotIsolated)]
    public sealed class AnaplanConnectorComponent : ServiceApplicationComponent<IServer>
    {
        private Timer _backgroundExportCheckTimer;

        public AnaplanConnectorComponent(ComponentInfo componentInfo) : base(componentInfo)
        {
        }

        public override void Start()
        {
            var asm = Assembly.GetExecutingAssembly();
            Container.Register(Types.FromAssembly(asm).BasedOn<IProvider>().WithServiceFromInterface().LifestyleSingleton());

            Container.Register(Component.For<IAnaplanClient>().ImplementedBy<AnaplanHttpClient>().LifestyleTransient());
            Container.Register(Component.For<IAnaplanClientFactory>().AsFactory().LifestyleSingleton());

            Container.Register(Component.For<IPersistentContainerStorage>().ImplementedBy<InMemoryContainerStorage>().LifestyleSingleton());

            Container.Register(Component.For<AnaplanDataExporter>().ImplementedBy<AnaplanDataExporter>().LifestyleSingleton());

            // Start timer to check each 5 minutes whether there are containers eligible for export.
            // Export containers each 4 hours.
            _backgroundExportCheckTimer = new Timer();
            _backgroundExportCheckTimer.Interval = TimeSpan.FromMinutes(5).TotalMilliseconds;
            _backgroundExportCheckTimer.AutoReset = true;
            _backgroundExportCheckTimer.Elapsed +=
                (sender, args) =>
                {
                    _ = Container.Resolve<AnaplanDataExporter>().ExportAllProviders(
                        ApplicationContext,
                        // Export data from containers each 4 hours
                        TimeSpan.FromHours(4));
                };
            _backgroundExportCheckTimer.Start();

            State = ServiceState.Started;
        }

        public override void Stop()
        {
            if (State == ServiceState.Stopped)
                return;

            _backgroundExportCheckTimer.Stop();
            _backgroundExportCheckTimer.Dispose();
            _backgroundExportCheckTimer = null;

            State = ServiceState.Stopped;
        }
    }
}
