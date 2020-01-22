using Metrics.Configuration;
using Metrics.CustomTracking;
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
    ///     - custom tracking
    /// </summary>
    internal class DiagnosticsObserver : IObserver<DiagnosticListener>
    {
        private readonly HttpConfiguration _httpConfiguration;
        private readonly MassTransitConfiguration _massTransitConfiguration;
        private readonly EntityFrameworkCoreConfiguration _entityFrameworkCoreConfiguration;
        private readonly CustomTrackingConfiguration _customTrackingConfiguration;
        private readonly ServiceConfiguration _serviceConfiguration;

        public DiagnosticsObserver(
            StatsdConfiguration statsdConfiguration,
            HttpConfiguration httpConfiguration,
            MassTransitConfiguration massTransitConfiguration,
            EntityFrameworkCoreConfiguration entityFrameworkCoreConfiguration,
            CustomTrackingConfiguration customTrackingConfiguration,
            ServiceConfiguration serviceConfiguration)
        {
            ConfigureStatsd(statsdConfiguration);

            _httpConfiguration = httpConfiguration;
            _massTransitConfiguration = massTransitConfiguration;
            _entityFrameworkCoreConfiguration = entityFrameworkCoreConfiguration;
            _customTrackingConfiguration = customTrackingConfiguration;
            _serviceConfiguration = serviceConfiguration;
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
                value.Subscribe(new HttpObserver(_httpConfiguration, _serviceConfiguration));
            }

            if (value.Name == "MassTransit" && _massTransitConfiguration.Enabled)
            {
                value.Subscribe(new MassTransitObserver(_massTransitConfiguration, _serviceConfiguration));
            }

            if (value.Name == "Microsoft.EntityFrameworkCore" && _entityFrameworkCoreConfiguration.Enabled)
            {
                value.Subscribe(new EntityFrameworkCoreObserver(_entityFrameworkCoreConfiguration, _serviceConfiguration));
            }

            if (value.Name == "CustomTracking" && _customTrackingConfiguration.Enabled)
            {
                value.Subscribe(new CustomTrackingObserver(_customTrackingConfiguration, _serviceConfiguration));
            }
        }
    }
}