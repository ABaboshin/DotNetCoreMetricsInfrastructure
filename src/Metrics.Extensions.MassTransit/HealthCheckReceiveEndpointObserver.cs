using MassTransit;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Metrics.Extensions.MassTransit
{
    /// <summary>
    /// health checks with IReceiveEndpointObserver
    /// based on https://github.com/MassTransit/MassTransit/blob/develop/src/Containers/MassTransit.AspNetCoreIntegration/HealthChecks/ReceiveEndpointHealthCheck.cs
    /// </summary>
    public class HealthCheckReceiveEndpointObserver : IReceiveEndpointObserver
    {
        private readonly ConcurrentDictionary<Uri, EndpointStatus> _endpoints = new ConcurrentDictionary<Uri, EndpointStatus>();
        private readonly DiagnosticListener _source = new DiagnosticListener(DiagnosticListenerUtil.HealthCheckEventName);

        public Task Completed(ReceiveEndpointCompleted completed)
        {
            return Task.CompletedTask;
        }

        public Task Faulted(ReceiveEndpointFaulted faulted)
        {
            var endpoint = GetEndpoint(faulted.InputAddress);

            endpoint.Ready = false;
            endpoint.LastException = faulted.Exception;

            _source.Write("hc", new { name = faulted.InputAddress.ToString(), healthy = false, faulted.Exception });

            return Task.CompletedTask;
        }

        public Task Ready(ReceiveEndpointReady ready)
        {
            GetEndpoint(ready.InputAddress).Ready = true;

            _source.Write("hc", new { name = ready.InputAddress.ToString(), healthy = true });

            _source.Write("hc", new { name = "rabbitmq://rabbitmq/queue-name", healthy = true });
            _source.Write("hc", new { name = "rabbitmq://rabbitmq/queue_name", healthy = true });

            return Task.CompletedTask;
        }

        EndpointStatus GetEndpoint(Uri inputAddress)
        {
            if (!_endpoints.ContainsKey(inputAddress))
                _endpoints.TryAdd(inputAddress, new EndpointStatus());

            return _endpoints[inputAddress];
        }

        class EndpointStatus
        {
            public bool Ready { get; set; }
            public Exception LastException { get; set; }
        }
    }
}
