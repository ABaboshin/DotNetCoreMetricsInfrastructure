using Metrics.Configuration;
using Metrics.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Metrics.UnitTests.Http
{
    public class HttpObserverTests
    {
        private const string HttpMetricsName = "httpmetrics";

        private readonly ILogger<HttpObserver> _logger;
        private readonly IMetricsSender _metricsSender;
        private readonly HttpObserver _httpObserver;
        private readonly TestObserver _testObserver;
        private readonly TestServer _testServer;

        public HttpObserverTests()
        {
            _logger = Substitute.For<ILogger<HttpObserver>>();

            _metricsSender = Substitute.For<IMetricsSender>();

            _httpObserver = new HttpObserver(
                new HttpConfiguration { Enabled = true, Name = HttpMetricsName },
                new ServiceConfiguration { Name = "srv1" },
                _logger,
                _metricsSender
            );

            _testObserver = new TestObserver("Microsoft.AspNetCore", _httpObserver);

            var builder = new WebHostBuilder()
                .UseStartup<Startup>()
                .ConfigureServices(services => {
                    DiagnosticListener.AllListeners.Subscribe(_testObserver);
                });

            _testServer = new TestServer(builder);
        }

        [Fact]
        public async Task It_should_capture_an_ok()
        {
            var request = _testServer.CreateRequest("/api/test/okresult");
            var response = await request.SendAsync("GET");
            await Task.Delay(1000);

            _metricsSender
                .Received()
                .Histogram<double>(HttpMetricsName, Arg.Any<double>(), Arg.Any<double>(), Arg.Is<string[]>(t => t.Any(s => s == "statusCode:200")));
        }

        [Fact]
        public async Task It_should_capture_an_exception()
        {
            var request = _testServer.CreateRequest("/api/test/exception");
            try
            {
                var response = await request.SendAsync("GET");
                await Task.Delay(1000);
            }
            catch (Exception)
            {
            }

            _metricsSender
                .Received()
                .Histogram<double>(HttpMetricsName, Arg.Any<double>(), Arg.Any<double>(), Arg.Is<string[]>(t => !t.All(s => s == "statusCode:200")));
        }
    }
}
