namespace Metrics.Configuration
{
    /// <summary>
    /// Configuration for entity framework queries tracking
    /// </summary>
    internal class EntityFrameworkCoreConfiguration
    {
        /// <summary>
        /// Section name in configuration
        /// </summary>
        public static readonly string SectionKey = "entityframeworkcore_metrics";

        /// <summary>
        /// enable tracking
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// metric name
        /// </summary>
        public string Name { get; set; } = "entityframeworkcore";
    }
}
