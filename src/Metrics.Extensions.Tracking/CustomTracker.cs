using System;
using System.Diagnostics;

namespace Metrics.Extensions.Tracking
{
    public class CustomTracker : ICustomTracker
    {
        public string ActivityName { get; set; }
        public string TraceIdentifier { get; set; }

        private DateTime _start;

        public void Start()
        {
            _start = DateTime.UtcNow;
        }

        public void Finish(Exception exception = null)
        {
            var source = new DiagnosticListener("CustomTracking");
            source.Write("track", new
            {
                Duration = (DateTime.UtcNow - _start).TotalMilliseconds,
                ActivityName,
                TraceIdentifier,
                Exception = exception
            });
        }
    }
}
