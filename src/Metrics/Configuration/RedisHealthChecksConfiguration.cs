﻿namespace Metrics.Configuration
{
    /// <summary>
    /// redis health checks configuration
    /// </summary>
    internal class  RedisHealthChecksConfiguration
    {
        /// <summary>
        /// enable redis health checks
        /// </summary>
        public bool Enabled { get; set; } = false;
        /// <summary>
        /// connection string
        /// </summary>
        public string ConnectionString { get; set; }
    }
}