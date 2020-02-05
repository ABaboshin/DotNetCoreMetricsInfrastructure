using Metrics.Configuration;
using Metrics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
            var customTrackingConfiguration = configuration.GetSection(CustomTrackingConfiguration.SectionKey).Get<CustomTrackingConfiguration>();
            var serviceConfiguration = configuration.GetSection(ServiceConfiguration.SectionKey).Get<ServiceConfiguration>();
            var healthChecksMetricsConfiguration = configuration.GetSection(HealthChecksMetricsConfiguration.SectionKey).Get<HealthChecksMetricsConfiguration>();

            builder.ConfigureServices(services => {
                var healthCheckBuilder = services.AddHealthChecks();

                if (healthChecksConfiguration.Sql.Enabled)
                {
                    healthCheckBuilder.AddSqlServer(healthChecksConfiguration.Sql.ConnectionString);
                }

                if (healthChecksConfiguration.Redis.Enabled)
                {
                    healthCheckBuilder.AddRedis(healthChecksConfiguration.Redis.ConnectionString);
                }

                if (healthChecksConfiguration.RabbitMQ.Enabled)
                {
                    healthCheckBuilder.AddRabbitMQ(healthChecksConfiguration.RabbitMQ.ConnectionString);
                }

                if (healthChecksConfiguration.Urls.Enabled)
                {
                    foreach (var url in healthChecksConfiguration.Urls.Urls)
                    {
                        healthCheckBuilder.AddCheck(url, new UrlHealthCheck(url));
                    }
                }

                services.AddSingleton<IStartupFilter>(serviceProvider => {
                    return new HealthChecksFilter(healthChecksConfiguration, serviceConfiguration);
                });

                var sp = services.BuildServiceProvider();
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

                DiagnosticListener.AllListeners.Subscribe(
                new DiagnosticsObserver(
                    statsdConfiguration,
                    httpConfiguration,
                    massTransitConfiguration,
                    entityFrameworkCoreConfiguration,
                    customTrackingConfiguration,
                    serviceConfiguration,
                    healthChecksMetricsConfiguration,
                    new StatsdMetricsSender(),
                    loggerFactory));
            });
        }
    }
}
