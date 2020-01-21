using Metrics.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metrics.HealthChecks
{
    /// <summary>
    /// healthcheck diagnsotic events observer
    /// </summary>
    internal class HealthChecksObserver : IObserver<KeyValuePair<string, object>>
    {
        private readonly HealthChecksConfiguration _healthChecksConfiguration;
        private readonly ServiceConfiguration _serviceConfiguration;

        public HealthChecksObserver(HealthChecksConfiguration healthChecksConfiguration, ServiceConfiguration serviceConfiguration)
        {
            _healthChecksConfiguration = healthChecksConfiguration;
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
            if (kv.Key == "healthcheck")
            {
                if (kv.Value is IDictionary<string, bool> dependencies)
                {
                    StatsdClient.DogStatsd.Gauge(_healthChecksConfiguration.Name,
                        dependencies.Values.All(v => v) ? 1 : 0,
                        tags: new[] { $"dependency:{_serviceConfiguration.Name}", $"service:{_serviceConfiguration.Name}" });

                    foreach (var dependency in dependencies)
                    {
                        StatsdClient.DogStatsd.Gauge(_healthChecksConfiguration.Name,
                            dependency.Value ? 1 : 0,
                            tags: new[] { $"dependency:{dependency.Key}", $"service:{_serviceConfiguration.Name}" });
                    }
                }
            }
        }
    }
}
