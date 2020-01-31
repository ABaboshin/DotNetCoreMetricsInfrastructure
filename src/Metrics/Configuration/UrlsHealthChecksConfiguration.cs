using System;
using System.Collections.Generic;
using System.Linq;

namespace Metrics.Configuration
{
    /// <summary>
    /// urls health checks configuration
    /// </summary>
    internal class UrlsHealthChecksConfiguration
    {
        /// <summary>
        /// enable url health checks
        /// </summary>
        public bool Enabled { get; set; } = false;
        /// <summary>
        /// urls to check
        /// </summary>
        public string UrlsList { get; set; }

        public IEnumerable<string> Urls
        {
            get
            {
                return UrlsList?.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries) ?? Enumerable.Empty<string>();
            }
        }
    }
}