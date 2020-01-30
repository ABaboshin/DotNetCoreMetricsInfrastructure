namespace Metrics.Configuration
{
    /// <summary>
    /// sql healthchecks
    /// </summary>
    internal class SqlHealthChecksConfiguration
    {
        /// <summary>
        /// enable sql health checks
        /// </summary>
        public bool Enabled { get; set; } = false;
        /// <summary>
        /// connection string
        /// </summary>
        public string ConnectionString { get; set; }
    }
}
