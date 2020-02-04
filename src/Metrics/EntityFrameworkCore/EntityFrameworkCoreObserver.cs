using Metrics.Configuration;
using Metrics.Extensions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Metrics.EntityFrameworkCore
{
    /// <summary>
    /// entity framework core diagnsotic events observer
    /// </summary>
    internal class EntityFrameworkCoreObserver : IObserver<KeyValuePair<string, object>>
    {
        private readonly ConcurrentDictionary<Guid, CommandInfo> info = new ConcurrentDictionary<Guid, CommandInfo>();
        private readonly EntityFrameworkCoreConfiguration _entityFrameworkCoreConfiguration;
        private readonly ServiceConfiguration _serviceConfiguration;
        private readonly ILogger<EntityFrameworkCoreObserver> _logger;

        public EntityFrameworkCoreObserver(EntityFrameworkCoreConfiguration entityFrameworkCoreConfiguration, ServiceConfiguration serviceConfiguration, ILogger<EntityFrameworkCoreObserver> logger)
        {
            _entityFrameworkCoreConfiguration = entityFrameworkCoreConfiguration;
            _serviceConfiguration = serviceConfiguration;
            _logger = logger;
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(KeyValuePair<string, object> kv)
        {
            // execution started
            if (kv.Key == RelationalEventId.CommandExecuting.Name && kv.Value is CommandEventData commandEventData)
            {
                info.TryAdd(commandEventData.CommandId, new CommandInfo { Data = commandEventData });
            }

            // execution successfully finished
            if (kv.Key == RelationalEventId.CommandExecuted.Name && kv.Value is CommandExecutedEventData commandExecutedEventData)
            {
                if (info.TryRemove(commandExecutedEventData.CommandId, out var existing))
                {
                    var duration = commandExecutedEventData.Duration.TotalMilliseconds;

                    _logger.LogDebug("EntityFrameworkCoreObserver {commandText} {service} {duration} {success}", commandExecutedEventData.Command.CommandText, _serviceConfiguration.Name, duration, true);

                    StatsdClient.DogStatsd.Histogram(_entityFrameworkCoreConfiguration.Name,
                        duration,
                        tags: new[] {
                            $"commandText:{commandExecutedEventData.Command.CommandText.EscapeTagValue()}",
                            $"service:{_serviceConfiguration.Name}",
                            $"success:True"
                        });
                }
            }

            // execution finished with an error
            if (kv.Key == RelationalEventId.CommandError.Name && kv.Value is CommandErrorEventData commandErrorEventData)
            {
                if (info.TryRemove(commandErrorEventData.CommandId, out var existing))
                {
                    var duration = commandErrorEventData.Duration.TotalMilliseconds;
                    var tags = new List<string> {
                        $"commandText:{commandErrorEventData.Command.CommandText.EscapeTagValue()}",
                        $"service:{_serviceConfiguration.Name}",
                        $"success:False"
                    };
                    tags.AddRange(commandErrorEventData.Exception.GetTags());

                    _logger.LogDebug(commandErrorEventData.Exception, "EntityFrameworkCoreObserver {commandText} {service} {duration} {success}", commandErrorEventData.Command.CommandText, _serviceConfiguration.Name, duration, false);

                    StatsdClient.DogStatsd.Histogram(_entityFrameworkCoreConfiguration.Name,
                        duration,
                        tags: tags.ToArray());
                }
            }
        }
    }
}
