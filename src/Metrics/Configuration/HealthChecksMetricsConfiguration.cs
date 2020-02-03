namespace Metrics.Configuration
{
    /// <summary>
    /// Configuration for healthchecks tracking
    /// </summary>
    internal class HealthChecksMetricsConfiguration
    {
        /// <summary>
        /// Section name in configuration
        /// </summary>
        public static readonly string SectionKey = "healthcheck_metrics";

        /// <summary>
        /// enable tracking
        /// </summary>

        public bool Enabled { get; set; }

        /// <summary>
        /// metric name
        /// </summary>
        public string Name { get; set; }
    }
}
