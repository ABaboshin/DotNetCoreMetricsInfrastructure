using Metrics.Configuration;
using Metrics.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Metrics.CustomTracking
{
    /// <summary>
    /// custom tracking diagnostics events ovserver
    /// </summary>
    internal class CustomTrackingObserver : IObserver<KeyValuePair<string, object>>
    {
        private readonly CustomTrackingConfiguration _customTrackingConfiguration;
        private readonly ServiceConfiguration _serviceConfiguration;
        private readonly ILogger<CustomTrackingObserver> _logger;

        public CustomTrackingObserver(CustomTrackingConfiguration customTrackingConfiguration, ServiceConfiguration serviceConfiguration, ILogger<CustomTrackingObserver> logger)
        {
            _customTrackingConfiguration = customTrackingConfiguration;
            _serviceConfiguration = serviceConfiguration;
            _logger = logger;
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(KeyValuePair<string, object> kv)
        {
            if (kv.Key == "track")
            {
                var duration = (double)kv.Value.GetType().GetTypeInfo().GetDeclaredProperty("Duration")?.GetValue(kv.Value);
                var activityName = (string)kv.Value.GetType().GetTypeInfo().GetDeclaredProperty("ActivityName")?.GetValue(kv.Value);
                var traceIdentifier = (string)kv.Value.GetType().GetTypeInfo().GetDeclaredProperty("TraceIdentifier")?.GetValue(kv.Value);
                var exception = (Exception)kv.Value.GetType().GetTypeInfo().GetDeclaredProperty("Exception")?.GetValue(kv.Value);

                var tags = new List<string> {
                            $"traceIdentifier:{traceIdentifier}",
                            $"activityName:{activityName}",
                            $"success:{exception is null}",
                            $"service:{_serviceConfiguration.Name}"
                        };

                if (exception != null)
                {
                    tags.AddRange(exception.GetTags());
                }

                var msg = "CustomTrackingObserver {traceIdentifier} {activityName} {service} {success} {duration}";
                if (exception != null)
                {
                    _logger.LogDebug(exception, msg, traceIdentifier, activityName, _serviceConfiguration.Name, exception is null, duration);
                }
                else
                {
                    _logger.LogDebug(msg, traceIdentifier, activityName, _serviceConfiguration.Name, exception is null);
                }

                StatsdClient.DogStatsd.Histogram(_customTrackingConfiguration.Name,
                        duration,
                        tags: tags.ToArray());
            }
        }
    }
}
