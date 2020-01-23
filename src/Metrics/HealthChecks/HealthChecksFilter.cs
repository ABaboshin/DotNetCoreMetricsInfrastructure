using Metrics.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System;

namespace Metrics.HealthChecks
{
    /// <summary>
    /// inject healthcheck middleware
    /// </summary>
    internal class HealthChecksFilter : IStartupFilter
    {
        private readonly HealthChecksConfiguration _healthChecksConfiguration;

        public HealthChecksFilter(HealthChecksConfiguration healthChecksConfiguration)
        {
            _healthChecksConfiguration = healthChecksConfiguration;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder => {
                builder.UseHealthChecks(_healthChecksConfiguration.Url);
                next(builder);
            };
        }
    }
}