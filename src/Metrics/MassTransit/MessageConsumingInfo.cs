using System;

namespace Metrics.MassTransit
{
    /// <summary>
    /// message consuming info
    /// </summary>
    internal class MessageConsumingInfo
    {
        /// <summary>
        /// start time
        /// </summary>
        public DateTime Start { get; set; }

        /// <summary>
        /// message type
        /// </summary>
        public string MessageType { get; set; }

        /// <summary>
        /// message id
        /// </summary>
        public Guid MessageId { get; set; }
    }
}
