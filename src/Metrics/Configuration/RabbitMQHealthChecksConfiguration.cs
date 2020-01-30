namespace Metrics.Configuration
{
    /// <summary>
    /// rabbitmq health checks configuration
    /// </summary>
    internal class RabbitMQHealthChecksConfiguration
    {
        /// <summary>
        /// enable rabbitmq health checks
        /// </summary>
        public bool Enabled { get; set; } = false;
        /// <summary>
        /// connection string
        /// </summary>
        public string ConnectionString { get; set; }
    }
}
