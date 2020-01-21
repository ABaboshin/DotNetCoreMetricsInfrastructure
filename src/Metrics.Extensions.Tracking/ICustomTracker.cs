using System;
using System.Collections.Generic;
using System.Text;

namespace Metrics.Extensions.Tracking
{
    /// <summary>
    /// a custom tracker
    /// </summary>
    public interface ICustomTracker
    {
        /// <summary>
        /// activity name
        /// </summary>
        string ActivityName { get; set; }

        /// <summary>
        /// trace identifier
        /// i.e. from the current http context
        /// </summary>
        string TraceIdentifier { get; set; }

        /// <summary>
        /// start activity tracking
        /// </summary>
        void Start();

        /// <summary>
        /// finish activity tracking and
        /// send diagnostic event
        /// </summary>
        /// <param name="exception"></param>
        void Finish(Exception exception = null);
    }
}
