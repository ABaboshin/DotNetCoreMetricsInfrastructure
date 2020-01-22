using Metrics.Configuration;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Metrics.HealthChecks
{
    /// <summary>
    /// health check middleware
    /// </summary>
    internal class HealthChecksMiddleware
    {
        private readonly HealthChecksConfiguration _healthChecksConfiguration;
        private readonly RequestDelegate _next;

        public HealthChecksMiddleware(RequestDelegate next, HealthChecksConfiguration healthChecksConfiguration)
        {
            _next = next;
            _healthChecksConfiguration = healthChecksConfiguration;
        }

        public async Task Invoke(HttpContext context)
        {
            /*
             * we have to port Microsoft.AspNetCore.Diagnostics.HealthChecks to .net core 2.1 here
                StatsdClient.DogStatsd.Gauge(_healthChecksConfiguration.Name,
                    dependencies.Values.All(v => v) ? 1 : 0,
                    tags: new[] { $"dependency:{_serviceConfiguration.Name}", $"service:{_serviceConfiguration.Name}" });

                foreach (var dependency in dependencies)
                {
                    StatsdClient.DogStatsd.Gauge(_healthChecksConfiguration.Name,
                        dependency.Value ? 1 : 0,
                        tags: new[] { $"dependency:{dependency.Key}", $"service:{_serviceConfiguration.Name}" });
                }
             */
            // 
            await context.Response.WriteAsync("ok");
        }
    }
}