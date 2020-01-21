using Metrics.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;

[assembly: HostingStartup(typeof(Metrics.MetricsInjector))]
namespace Metrics
{
    /// <summary>
    /// IHostingStartup implementation to inject the metrics processing
    /// </summary>
    public class MetricsInjector : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            var statsdConfiguration = configuration.GetSection(StatsdConfiguration.SectionKey).Get<StatsdConfiguration>();
            var httpConfiguration = configuration.GetSection(HttpConfiguration.SectionKey).Get<HttpConfiguration>();
            var massTransitConfiguration = configuration.GetSection(MassTransitConfiguration.SectionKey).Get<MassTransitConfiguration>();
            var entityFrameworkCoreConfiguration = configuration.GetSection(EntityFrameworkCoreConfiguration.SectionKey).Get<EntityFrameworkCoreConfiguration>();
            var healthChecksConfiguration = configuration.GetSection(HealthChecksConfiguration.SectionKey).Get<HealthChecksConfiguration>();
            var serviceConfiguration = configuration.GetSection(ServiceConfiguration.SectionKey).Get<ServiceConfiguration>();

            DiagnosticListener.AllListeners.Subscribe(
                new DiagnosticsObserver(
                    statsdConfiguration,
                    httpConfiguration,
                    massTransitConfiguration,
                    entityFrameworkCoreConfiguration,
                    healthChecksConfiguration,
                    serviceConfiguration));
        }
    }
}
