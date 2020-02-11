using Metrics.Configuration;
using Metrics.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Metrics.UnitTests.EfCore
{
    public class EntityFrameworkCoreListenerTests : IDisposable
    {
        private const string EfCoreMetricsName = "efcoremetrics";
        private readonly DbConnection _connection;
        private readonly FakeDbContext _dbContext;

        private readonly ILogger<EntityFrameworkCoreObserver> _logger;
        private readonly IMetricsSender _metricsSender;
        private readonly EntityFrameworkCoreObserver _efCoreObserver;
        private readonly TestObserver _testObserver;

        public EntityFrameworkCoreListenerTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<FakeDbContext>()
                .UseSqlite(_connection)
                .Options;

            _dbContext = new FakeDbContext(options);

            _logger = NSubstitute.Substitute.For<ILogger<EntityFrameworkCoreObserver>>();

            _metricsSender = NSubstitute.Substitute.For<IMetricsSender>();

            _efCoreObserver = new EntityFrameworkCoreObserver(
                new EntityFrameworkCoreConfiguration { Enabled = true, Name = EfCoreMetricsName },
                new ServiceConfiguration { Name = "srv1" },
                _logger,
                _metricsSender
                );

            _testObserver = new TestObserver("Microsoft.EntityFrameworkCore", _efCoreObserver);

            DiagnosticListener.AllListeners.Subscribe(_testObserver);
        }

        public void Dispose()
        {
            _connection?.Close();
            _dbContext?.Dispose();
        }

        [Fact]
        public async Task It_should_capture_an_exception()
        {
            try
            {
                await _dbContext.Database.ExecuteSqlRawAsync("SELECT * FROM FakeTable");
            }
            catch
            {
                // ignore
            }

            _metricsSender
                .Received()
                .Histogram<double>(EfCoreMetricsName, Arg.Any<double>(), Arg.Any<double>(), Arg.Is<string[]>(t => t.Any(s => s == "success:False")));
        }

        [Fact]
        public async Task It_should_capture_an_ok()
        {
            await _dbContext.Database.ExecuteSqlRawAsync("SELECT 1");

            _metricsSender
                .Received()
                .Histogram<double>(EfCoreMetricsName, Arg.Any<double>(), Arg.Any<double>(), Arg.Is<string[]>(t => t.Any(s => s == "success:True")));
        }
    }
}
