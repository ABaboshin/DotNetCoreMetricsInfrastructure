using Metrics.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Metrics.HealthChecks
{
    /// <summary>
    /// inject healthcheck middleware
    /// </summary>
    internal class HealthChecksFilter : IStartupFilter
    {
        private readonly HealthChecksConfiguration _healthChecksConfiguration;
        private readonly ServiceConfiguration _serviceConfiguration;
        private readonly DiagnosticListener _source = new DiagnosticListener("HealthCheck");

        public HealthChecksFilter(HealthChecksConfiguration healthChecksConfiguration, ServiceConfiguration serviceConfiguration)
        {
            _healthChecksConfiguration = healthChecksConfiguration;
            _serviceConfiguration = serviceConfiguration;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder => {
                builder.UseHealthChecks(
                    _healthChecksConfiguration.Url,
                    new HealthCheckOptions
                    {
                        ResponseWriter = WriteResponse
            }
                );
                next(builder);
            };
        }

        private Task WriteResponse(HttpContext httpContext, HealthReport result)
        {
            httpContext.Response.ContentType = MediaTypeNames.Application.Json;

            foreach (var dependency in result.Entries)
            {
                _source.Write("hc", new { name = dependency.Key, healthy = dependency.Value.Status == HealthStatus.Healthy, dependency.Value.Exception });
            }

            var data = new
            {
                _serviceConfiguration.Name,
                result.Status,
                result.TotalDuration,
                Healthy = result.Status == HealthStatus.Healthy,
                Dependencies = result.Entries.Select(e => new {
                    Name = e.Key,
                    e.Value.Status,
                    Healthy = e.Value.Status == HealthStatus.Healthy,
                    e.Value.Data,
                    e.Value.Description,
                    e.Value.Duration,
                    e.Value.Exception
                })
            };
            
            var json = JsonConvert.SerializeObject
            (
                value: data,
                settings: new JsonSerializerSettings()
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                }
            );

            return httpContext.Response.WriteAsync(json);
        }
    }
}