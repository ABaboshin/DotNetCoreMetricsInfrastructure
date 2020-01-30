namespace Metrics.Configuration
{
    /// <summary>
    /// Configuration for healthchecks
    /// </summary>
    internal class HealthChecksConfiguration
    {
        /// <summary>
        /// Section name in configuration
        /// </summary>
        public static readonly string SectionKey = "healthcheck";

        /// <summary>
        /// healthcheck url
        /// </summary>
        public string Url { get; set; } = "/api/hc";

        /// <summary>
        /// sql health checks
        /// </summary>
        public SqlHealthChecksConfiguration Sql { get; set; } = new SqlHealthChecksConfiguration();

        /// <summary>
        /// redis health checks
        /// </summary>
        public RedisHealthChecksConfiguration Redis { get; set; } = new RedisHealthChecksConfiguration();

        /// <summary>
        /// redis health checks
        /// </summary>
        public RabbitMQHealthChecksConfiguration RabbitMQ { get; set; } = new RabbitMQHealthChecksConfiguration();

        /// <summary>
        /// health checks metrics
        /// </summary>
        public HealthChecksMetricsConfiguration Mertrics { get; set; } = new HealthChecksMetricsConfiguration();
    }
}
