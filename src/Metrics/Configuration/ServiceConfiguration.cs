namespace Metrics.Configuration
{
    /// <summary>
    /// service configuration
    /// </summary>
    internal class ServiceConfiguration
    {
        /// <summary>
        /// Section name in configuration
        /// </summary>
        public static readonly string SectionKey = "service";

        /// <summary>
        /// metric name
        /// </summary>
        public string Name { get; set; } = "name";
    }
}
