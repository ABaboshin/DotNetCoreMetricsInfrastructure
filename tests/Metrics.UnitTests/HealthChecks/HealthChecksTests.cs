using FluentAssertions;
using MassTransit.Testing;
using Metrics.Configuration;
using Metrics.Extensions.MassTransit;
using Metrics.HealthChecks;
using Metrics.UnitTests.Http;
using Metrics.UnitTests.MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSubstitute;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Metrics.UnitTests.HealthChecks
{
    public class HealthChecksTests
    {
        private const string HealthChecksMetricsName = "hcmetrics";
        private const string ServiceName = "srv1";

        private readonly ILogger<HealthChecksObserver> _logger;
        private readonly IMetricsSender _metricsSender;
        private readonly HealthChecksObserver _hcObserver;
        private readonly TestObserver _testObserver;

        private readonly TestServer _testServer;

        public HealthChecksTests()
        {
            _logger = Substitute.For<ILogger<HealthChecksObserver>>();

            _metricsSender = Substitute.For<IMetricsSender>();

            var serviceConfiguration = new ServiceConfiguration { Name = ServiceName };
            _hcObserver = new HealthChecksObserver(
                new HealthChecksMetricsConfiguration { Enabled = true, Name = HealthChecksMetricsName },
                serviceConfiguration,
                _logger,
                _metricsSender
            );

            _testObserver = new TestObserver("HealthCheck", _hcObserver);
            DiagnosticListener.AllListeners.Subscribe(_testObserver);

            var builder = new WebHostBuilder()
                .UseStartup<Startup>()
                .ConfigureServices(services => {
                    var healthCheckBuilder = services.AddHealthChecks();

                    services.AddSingleton((System.Func<System.IServiceProvider, IStartupFilter>)(serviceProvider => {
                        return new HealthChecksFilter(
                            new HealthChecksConfiguration(),
                            serviceConfiguration);
                    }));
                });

            _testServer = new TestServer(builder);
        }

        [Fact]
        public async Task It_should_capture_masstransit_health_checks()
        {
            var harness = new InMemoryTestHarness();

            harness.OnConfigureInMemoryReceiveEndpoint += cfg => {
                cfg.ConnectReceiveEndpointObserver(new HealthCheckReceiveEndpointObserver());
            };

            var consumerHarness = harness.Consumer<OkConsumer>();

            await harness.Start();

            await harness.InputQueueSendEndpoint.Send(new OkMessage());

            await harness.Stop();

            await Task.Delay(5000);

            var calls = _metricsSender.ReceivedCalls();

            calls.Count().Should().Be(1);

            var arguments = calls.First().GetArguments();
            arguments.Count().Should().Be(4);

            arguments[0].ToString().Should().Be(HealthChecksMetricsName);
            (arguments[3] as string[]).Should().Contain("success:True");
        }

        [Fact]
        public async Task It_should_start_health_checks()
        {
            var request = _testServer.CreateRequest("/api/hc");
            var response = await request.SendAsync("GET");
            response.StatusCode.Should().Be(200);
            var content = await response.Content.ReadAsStringAsync();
            var json = (JObject)JsonConvert.DeserializeObject(content);

            json.Value<string>("name").Should().Be(ServiceName);
            json.Value<bool>("healthy").Should().Be(true);
        }
    }
}
