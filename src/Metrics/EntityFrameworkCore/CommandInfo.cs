using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Metrics.EntityFrameworkCore
{
    /// <summary>
    /// command info
    /// </summary>
    internal class CommandInfo
    {
        /// <summary>
        /// command event data
        /// </summary>
        public CommandEventData Data { get; set; }
    }
}
