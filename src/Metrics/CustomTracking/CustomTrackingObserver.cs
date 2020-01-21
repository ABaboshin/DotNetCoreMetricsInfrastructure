using Metrics.Configuration;
using Metrics.Extensions;
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

        public CustomTrackingObserver(CustomTrackingConfiguration customTrackingConfiguration, ServiceConfiguration serviceConfiguration)
        {
            _customTrackingConfiguration = customTrackingConfiguration;
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

                StatsdClient.DogStatsd.Histogram(_customTrackingConfiguration.Name,
                        duration,
                        tags: tags.ToArray());
            }
        }
    }
}
