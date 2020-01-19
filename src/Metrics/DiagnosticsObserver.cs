using Metrics.Configuration;
using Metrics.EntityFrameworkCore;
using Metrics.Http;
using Metrics.MassTransit;
using StatsdClient;
using System;
using System.Diagnostics;

namespace Metrics
{
    /// <summary>
    /// Observe diagnostic events for:
    ///     - http requests
    ///     - entity framework core queries
    ///     - masstransit message consuming
    /// </summary>
    internal class DiagnosticsObserver : IObserver<DiagnosticListener>
    {
        private readonly HttpConfiguration _httpConfiguration;
        private readonly MassTransitConfiguration _massTransitConfiguration;
        private readonly EntityFrameworkCoreConfiguration _entityFrameworkCoreConfiguration;

        public DiagnosticsObserver(
            StatsdConfiguration statsdConfiguration,
            HttpConfiguration httpConfiguration,
            MassTransitConfiguration massTransitConfiguration,
            EntityFrameworkCoreConfiguration entityFrameworkCoreConfiguration)
        {
            if (statsdConfiguration is null)
            {
                throw new ArgumentNullException("statsdConfiguration");
            }

            if (httpConfiguration is null)
            {
                throw new ArgumentNullException("httpConfiguration");
            }

            ConfigureStatsd(statsdConfiguration);

            _httpConfiguration = httpConfiguration;
            _massTransitConfiguration = massTransitConfiguration;
            _entityFrameworkCoreConfiguration = entityFrameworkCoreConfiguration;
        }

        private void ConfigureStatsd(StatsdConfiguration statsdConfiguration)
        {
            DogStatsd.Configure(new StatsdConfig
            {
                StatsdServerName = statsdConfiguration.Server,
                StatsdPort = statsdConfiguration.Port
            });
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(DiagnosticListener value)
        {
            if (value.Name == "Microsoft.AspNetCore" && _httpConfiguration.Enabled)
            {
                value.Subscribe(new HttpObserver(_httpConfiguration));
            }

            if (value.Name == "MassTransit" && _massTransitConfiguration.Enabled)
            {
                value.Subscribe(new MassTransitObserver(_massTransitConfiguration));
            }

            if (value.Name == "Microsoft.EntityFrameworkCore" && _entityFrameworkCoreConfiguration.Enabled)
            {
                value.Subscribe(new EntityFrameworkCoreObserver(_entityFrameworkCoreConfiguration));
            }
        }
    }
}