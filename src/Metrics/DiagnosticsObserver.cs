using Metrics.Configuration;
using Metrics.CustomTracking;
using Metrics.EntityFrameworkCore;
using Metrics.HealthChecks;
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
        private readonly HealthChecksMetricsConfiguration _healthChecksMetricsConfiguration;

        private HttpObserver httpObserver;
        private MassTransitObserver mtObserver;
        private EntityFrameworkCoreObserver efObserver;
        private CustomTrackingObserver ctObserver;

        public DiagnosticsObserver(
            StatsdConfiguration statsdConfiguration,
            HttpConfiguration httpConfiguration,
            MassTransitConfiguration massTransitConfiguration,
            EntityFrameworkCoreConfiguration entityFrameworkCoreConfiguration,
            CustomTrackingConfiguration customTrackingConfiguration,
            ServiceConfiguration serviceConfiguration,
            HealthChecksMetricsConfiguration healthChecksMetricsConfiguration)
        {
            ConfigureStatsd(statsdConfiguration);

            _httpConfiguration = httpConfiguration;
            _massTransitConfiguration = massTransitConfiguration;
            _entityFrameworkCoreConfiguration = entityFrameworkCoreConfiguration;
            _customTrackingConfiguration = customTrackingConfiguration;
            _serviceConfiguration = serviceConfiguration;
            _healthChecksMetricsConfiguration = healthChecksMetricsConfiguration;
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
                httpObserver = new HttpObserver(_httpConfiguration, _serviceConfiguration);
                value.Subscribe(httpObserver);
            }

            if (value.Name == "MetricMassTransit" && _massTransitConfiguration.Enabled)
            {
                mtObserver = new MassTransitObserver(_massTransitConfiguration, _serviceConfiguration);
                value.Subscribe(mtObserver);
            }

            if (value.Name == "Microsoft.EntityFrameworkCore" && _entityFrameworkCoreConfiguration.Enabled)
            {
                efObserver = new EntityFrameworkCoreObserver(_entityFrameworkCoreConfiguration, _serviceConfiguration);
                value.Subscribe(efObserver);
            }

            if (value.Name == "CustomTracking" && _customTrackingConfiguration.Enabled)
            {
                ctObserver = new CustomTrackingObserver(_customTrackingConfiguration, _serviceConfiguration);
                value.Subscribe(ctObserver);
            }

            if (value.Name == "HealthCheck" && _healthChecksMetricsConfiguration.Enabled)
            {
                value.Subscribe(new HealthChecksObserver(_healthChecksMetricsConfiguration, _serviceConfiguration));
            }
        }
    }
}