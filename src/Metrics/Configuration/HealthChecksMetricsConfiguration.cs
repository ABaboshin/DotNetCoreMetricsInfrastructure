namespace Metrics.Configuration
{
    /// <summary>
    /// Configuration for healthchecks tracking
    /// </summary>
    internal class HealthChecksMetricsConfiguration
    {
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
