using Metrics.Configuration;
using Metrics.Extensions;
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
        private HealthChecksMetricsConfiguration _healthChecksMetricsConfiguration;
        private ServiceConfiguration _serviceConfiguration;

        public HealthChecksObserver(HealthChecksMetricsConfiguration healthChecksMetricsConfiguration, ServiceConfiguration serviceConfiguration)
        {
            _healthChecksMetricsConfiguration = healthChecksMetricsConfiguration;
            _serviceConfiguration = serviceConfiguration;
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
                            $"service:{_serviceConfiguration.Name}"
                        };

                if (exception != null)
                {
                    tags.AddRange(exception.GetTags());
                }

                StatsdClient.DogStatsd.Histogram(_healthChecksMetricsConfiguration.Name,
                        healthy ? 1 : 0,
                        tags: tags.ToArray());
            }
        }
    }
}