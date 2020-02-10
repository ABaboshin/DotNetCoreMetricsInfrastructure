using Metrics.Configuration;
using Metrics.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Metrics.HealthChecks
{
    /// <summary>
    /// health check event observer
    /// </summary>
    internal class HealthChecksObserver : IObserver<KeyValuePair<string, object>>
    {
        private readonly HealthChecksMetricsConfiguration _healthChecksMetricsConfiguration;
        private readonly ServiceConfiguration _serviceConfiguration;
        private readonly ILogger<HealthChecksObserver> _logger;
        private readonly IMetricsSender _metricsSender;

        public HealthChecksObserver(HealthChecksMetricsConfiguration healthChecksMetricsConfiguration, ServiceConfiguration serviceConfiguration, ILogger<HealthChecksObserver> logger, IMetricsSender metricsSender)
        {
            _healthChecksMetricsConfiguration = healthChecksMetricsConfiguration;
            _serviceConfiguration = serviceConfiguration;
            _logger = logger;
            _metricsSender = metricsSender;
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(KeyValuePair<string, object> kv)
        {
            if (kv.Key == "hc")
            {
                var name = (string)kv.Value.GetType().GetTypeInfo().GetDeclaredProperty("name")?.GetValue(kv.Value);
                var healthy = (bool)kv.Value.GetType().GetTypeInfo().GetDeclaredProperty("healthy")?.GetValue(kv.Value);
                var exception = (Exception)kv.Value.GetType().GetTypeInfo().GetDeclaredProperty("Exception")?.GetValue(kv.Value);

                var tags = new List<string> {
                            $"dependency:{name}",
                            $"service:{_serviceConfiguration.Name}",
                            $"success:{healthy}"
                        };

                if (exception != null)
                {
                    tags.AddRange(exception.GetTags());
                }

                var msg = "HealthChecksObserver {dependency} {service} {healthy}";
                if (exception != null)
                {
                    _logger.LogDebug(exception, msg, name, _serviceConfiguration.Name, healthy);
                }
                else
                {
                    _logger.LogDebug(msg, name, _serviceConfiguration.Name, healthy);
                }

                _metricsSender.Histogram(_healthChecksMetricsConfiguration.Name,
                        healthy ? 1 : 0,
                        tags: tags.ToArray());
            }
        }
    }
}