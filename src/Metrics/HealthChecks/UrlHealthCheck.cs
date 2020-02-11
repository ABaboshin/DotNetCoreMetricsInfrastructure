using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Metrics.HealthChecks
{
    internal class UrlHealthCheck : IHealthCheck
    {
        private readonly string _url;

        public UrlHealthCheck(string url)
        {
            _url = url;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
                var response = await httpClient.GetAsync(_url).ConfigureAwait(false);
                var body = await response.Content?.ReadAsStringAsync();
                dynamic obj = null;
                
                try
                {
                    obj = (dynamic)JsonSerializer.Deserialize(body, typeof(object));
                    obj.url = _url;
                }
                catch (Exception)
                {
                    obj = new { url = _url, body };
                }

                var data = new Dictionary<string, object>();
                foreach (PropertyDescriptor prop in TypeDescriptor.GetProperties(obj))
                {
                    data.Add(prop.Name, prop.GetValue(obj));
                }

                return HealthCheckResult.Healthy(data: data);
            }
            catch (Exception exception)
            {
                var data = new Dictionary<string, object> {
                    { "url", _url },
                    { "exception", exception }
                };
                return HealthCheckResult.Unhealthy(exception: exception, data: data);
            }
        }
    }
}
