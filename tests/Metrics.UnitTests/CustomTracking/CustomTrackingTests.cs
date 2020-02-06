using Metrics.Configuration;
using Metrics.CustomTracking;
using Metrics.Extensions.Tracking;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Diagnostics;
using System.Linq;
using Xunit;

namespace Metrics.UnitTests.CustomTracking
{
    public class CustomTrackingTests
    {
        private const string CustomTrackingMetricsName = "ctmetrics";

        private readonly ILogger<CustomTrackingObserver> _logger;
        private readonly IMetricsSender _metricsSender;
        private readonly CustomTrackingObserver _ctObserver;
        private readonly TestObserver _testObserver;

        public CustomTrackingTests()
        {
            _logger = NSubstitute.Substitute.For<ILogger<CustomTrackingObserver>>();

            _metricsSender = NSubstitute.Substitute.For<IMetricsSender>();

            _ctObserver = new CustomTrackingObserver(
                new CustomTrackingConfiguration { Enabled = true, Name = CustomTrackingMetricsName },
                new ServiceConfiguration { Name = "srv1" },
                _logger,
                _metricsSender
                );

            _testObserver = new TestObserver("CustomTracking", _ctObserver);

            DiagnosticListener.AllListeners.Subscribe(_testObserver);
        }

        [Fact]
        public void It_should_capture_an_exception()
        {
            var customTracker = new CustomTracker();
            customTracker.Start();
            customTracker.Finish(new Exception());

            _metricsSender
                .Received()
                .Histogram<double>(CustomTrackingMetricsName, Arg.Any<double>(), Arg.Any<double>(), Arg.Is<string[]>(t => t.Any(s => s == "success:False")));
        }

        [Fact]
        public void It_should_capture_an_ok()
        {
            var customTracker = new CustomTracker();
            customTracker.Start();
            customTracker.Finish();

            _metricsSender
                .Received()
                .Histogram<double>(CustomTrackingMetricsName, Arg.Any<double>(), Arg.Any<double>(), Arg.Is<string[]>(t => t.Any(s => s == "success:True")));
        }
    }
}
