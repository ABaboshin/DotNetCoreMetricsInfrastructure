using System;
using System.Collections.Generic;
using System.Text;

namespace Metrics.Configuration
{
    /// <summary>
    /// Configuration for custom tracking
    /// </summary>
    internal class CustomTrackingConfiguration
    {
        /// <summary>
        /// Section name in configuration
        /// </summary>
        public static readonly string SectionKey = "customtracking_metrics";

        /// <summary>
        /// enable tracking
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// metric name
        /// </summary>
        public string Name { get; set; } = "customtracking";
    }
}
