using System;
using System.Collections.Generic;
using System.Text;

namespace Metrics
{
    /// <summary>
    /// Configuration for http requests tracking
    /// </summary>
    internal class MassTransitConfiguration
    {
        /// <summary>
        /// Section name in configuration
        /// </summary>
        public static readonly string SectionKey = "masstransit_metrics";

        /// <summary>
        /// enable tracking
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// metric name
        /// </summary>
        public string Name { get; set; } = "masstransit";
    }
}
