using Metrics.Configuration;
using Metrics.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace Metrics.MassTransit
{
    /// <summary>
    /// MassTransit diagnsotic events observer.
    /// See Metrics.Extensions.MassTransit
    /// </summary>
    internal class MassTransitObserver : IObserver<KeyValuePair<string, object>>
    {
        private readonly ConcurrentDictionary<Guid, MessageConsumingInfo> info = new ConcurrentDictionary<Guid, MessageConsumingInfo>();
        private readonly MassTransitConfiguration _massTransitConfiguration;
        private readonly ServiceConfiguration _serviceConfiguration;
        private readonly ILogger<MassTransitObserver> _logger;
        private readonly IMetricsSender _metricsSender;

        public MassTransitObserver(MassTransitConfiguration massTransitConfiguration, ServiceConfiguration serviceConfiguration, ILogger<MassTransitObserver> logger, IMetricsSender metricsSender)
        {
            _massTransitConfiguration = massTransitConfiguration;
            _serviceConfiguration = serviceConfiguration;
            _logger = logger;
            _metricsSender = metricsSender;
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(KeyValuePair<string, object> value)
        {
            if (value.Key.StartsWith("Custom Consuming"))
            {
                ProcessMessageConsuming(value);
            }
        }

        private void ProcessMessageConsuming(KeyValuePair<string, object> value)
        {
            if (value.Key.EndsWith("Start"))
            {
                ProcessMessageConsumingStarted(value.Value);
            }

            if (value.Key.EndsWith("Stop"))
            {
                ProcessMessageConsumingStoped(value.Value);
            }
        }

        /// <summary>
        /// process message consuming started event
        /// </summary>
        /// <param name="value"></param>
        private void ProcessMessageConsumingStarted(object value)
        {
            var context = value.GetType().GetTypeInfo().GetDeclaredProperty("context")?.GetValue(value);
            if (context != null)
            {
                var message = context.GetType().GetTypeInfo().GetDeclaredProperty("Message")?.GetValue(context);
                var messageId = context.GetType().GetTypeInfo().GetDeclaredProperty("MassTransit.MessageContext.MessageId")?.GetValue(context);

                if (message != null && messageId != null)
                {
                    info.TryAdd((Guid)messageId, new MessageConsumingInfo { Start = DateTime.UtcNow, MessageId = (Guid)messageId, MessageType = message.GetType().FullName });
                }
            }
        }

        /// <summary>
        /// process message consuming stoped event
        /// </summary>
        /// <param name="value"></param>
        private void ProcessMessageConsumingStoped(object value)
        {
            var context = value.GetType().GetTypeInfo().GetDeclaredProperty("context")?.GetValue(value);
            if (context != null)
            {
                var message = context.GetType().GetTypeInfo().GetDeclaredProperty("Message")?.GetValue(context);
                var messageId = context.GetType().GetTypeInfo().GetDeclaredProperty("MassTransit.MessageContext.MessageId")?.GetValue(context);
                var success = (bool)value.GetType().GetTypeInfo().GetDeclaredProperty("success")?.GetValue(value);

                Exception exception = null;

                if (!success)
                {
                    exception = (Exception)value.GetType().GetTypeInfo().GetDeclaredProperty("exception")?.GetValue(value);
                }

                if (message != null && messageId != null && info.TryRemove((Guid)messageId, out var existing))
                {
                    var end = DateTime.UtcNow;

                    var tags = new List<string> {
                            $"messageType:{existing.MessageType}",
                            $"messageId:{existing.MessageId}",
                            $"success:{success}",
                            $"service:{_serviceConfiguration.Name}"
                        };

                    if (!success && exception != null)
                    {
                        tags.AddRange(exception.GetTags());
                    }

                    var msg = "MassTransitObserver {messageType} {messageId} {success} {service}";
                    if (exception != null)
                    {
                        _logger.LogDebug(exception, msg, existing.MessageType, existing.MessageId, success, _serviceConfiguration.Name);
                    }
                    else
                    {
                        _logger.LogDebug(msg, existing.MessageType, existing.MessageId, success, _serviceConfiguration.Name);
                    }

                    _metricsSender.Histogram(_massTransitConfiguration.Name,
                        (end - existing.Start).TotalMilliseconds,
                        tags: tags.ToArray());
                }
            }
        }
    }
}
