using MassTransit;
using MassTransit.Testing;
using Metrics.Configuration;
using Metrics.Extensions.MassTransit;
using Metrics.MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Metrics.UnitTests.MassTransit
{
    public class MassTransitObserverTests
    {
        private const string MassTransitMetricsName = "masstransitmetrics";

        private readonly ILogger<MassTransitObserver> _logger;
        private readonly IMetricsSender _metricsSender;
        private readonly MassTransitObserver _mtObserver;
        private readonly TestObserver _testObserver;

        public MassTransitObserverTests()
        {
            _logger = Substitute.For<ILogger<MassTransitObserver>>();

            _metricsSender = Substitute.For<IMetricsSender>();

            _mtObserver = new MassTransitObserver(
                new MassTransitConfiguration { Enabled = true, Name = MassTransitMetricsName },
                new ServiceConfiguration { Name = "srv1" },
                _logger,
                _metricsSender
            );

            _testObserver = new TestObserver("MetricMassTransit", _mtObserver);
            DiagnosticListener.AllListeners.Subscribe(_testObserver);
        }

        [Fact]
        public async Task It_should_capture_an_ok()
        {
            var harness = new InMemoryTestHarness();
            harness.OnConfigureInMemoryBus += cfg => {
                cfg.AddPipeSpecification(new TrackConsumingSpecification<ConsumeContext>());
            };
            
            var consumerHarness = harness.Consumer<OkConsumer>();
            await harness.Start();

            await harness.InputQueueSendEndpoint.Send(new OkMessage());

            await Task.Delay(10000);

            await harness.Stop();

            _metricsSender
                .Received()
                .Histogram<double>(MassTransitMetricsName, Arg.Any<double>(), Arg.Any<double>(), Arg.Is<string[]>(t => t.Any(s => s == "success:True")));
        }

        [Fact]
        public async Task It_should_capture_an_exception()
        {
            var harness = new InMemoryTestHarness();
            harness.OnConfigureInMemoryBus += cfg => {
                cfg.AddPipeSpecification(new TrackConsumingSpecification<ConsumeContext>());
            };

            var consumerHarness = harness.Consumer<ExceptionConsumer>();
            await harness.Start();

            await harness.InputQueueSendEndpoint.Send(new ExceptionMessage());

            await Task.Delay(10000);

            await harness.Stop();

            _metricsSender
                .Received()
                .Histogram<double>(MassTransitMetricsName, Arg.Any<double>(), Arg.Any<double>(), Arg.Is<string[]>(t => t.Any(s => s == "success:False")));
        }
    }
}
