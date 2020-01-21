using Metrics.Configuration;
using Metrics.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace Metrics.Http
{
    /// <summary>
    /// http request diagnsotic events observer
    /// </summary>
    internal class HttpObserver : IObserver<KeyValuePair<string, object>>
    {
        private readonly ConcurrentDictionary<string, RequestInfo> info = new ConcurrentDictionary<string, RequestInfo>();
        private readonly HttpConfiguration _httpConfiguration;
        private readonly ServiceConfiguration _serviceConfiguration;

        public HttpObserver(HttpConfiguration httpConfiguration, ServiceConfiguration serviceConfiguration)
        {
            _httpConfiguration = httpConfiguration;
            _serviceConfiguration = serviceConfiguration;
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(KeyValuePair<string, object> kv)
        {
            if (kv.Key == "Microsoft.AspNetCore.Hosting.HttpRequestIn.Start")
            {
                ProcessStartEvent(kv.Value);
            }

            if (kv.Key == "Microsoft.AspNetCore.Mvc.BeforeAction")
            {
                ProcessBeforeAction(kv.Value);
            }

            if (kv.Key == "Microsoft.AspNetCore.Hosting.HttpRequestIn.Stop")
            {
                ProcessStopEvent(kv.Value);
            }

            if (kv.Key == "Microsoft.AspNetCore.Diagnostics.UnhandledException")
            {
                ProcessUnhandledException(kv.Value);
            }
        }

        /// <summary>
        /// process an unhandled exception
        /// </summary>
        /// <param name="value"></param>
        private void ProcessUnhandledException(object value)
        {
            var httpContext = value.GetType().GetTypeInfo().GetDeclaredProperty("httpContext")?.GetValue(value) as HttpContext;
            var exception = value.GetType().GetTypeInfo().GetDeclaredProperty("exception")?.GetValue(value) as Exception;
            if (httpContext != null)
            {
                var traceIdentifier = httpContext.TraceIdentifier;

                if (info.TryGetValue(traceIdentifier, out var existing))
                {
                    info.TryUpdate(traceIdentifier,
                        new RequestInfo
                        {
                            ActionName = existing.ActionName,
                            ControllerName = existing.ControllerName,
                            Start = existing.Start,
                            Exception = exception
                        },
                        existing);
                }
            }
        }

        /// <summary>
        /// process request finished
        /// </summary>
        /// <param name="value"></param>
        private void ProcessStopEvent(object value)
        {
            var httpContext = value.GetType().GetTypeInfo().GetDeclaredProperty("HttpContext")?.GetValue(value) as HttpContext;
            if (httpContext != null)
            {
                var traceIdentifier = httpContext.TraceIdentifier;

                if (info.TryRemove(traceIdentifier, out var existing))
                {
                    var end = DateTime.UtcNow;
                    var tags = new List<string> {
                            $"action:{existing.ActionName ?? ""}",
                            $"controller:{existing.ControllerName ?? ""}",
                            $"statusCode:{httpContext.Response.StatusCode}",
                            $"traceIdentifier:{traceIdentifier}",
                            $"service:{_serviceConfiguration.Name}"
                        };
                    if (existing.Exception != null)
                    {
                        tags.AddRange(existing.Exception.GetTags());
                    }
                    
                    StatsdClient.DogStatsd.Histogram(_httpConfiguration.Name,
                        (end - existing.Start).TotalMilliseconds,
                        tags: tags.ToArray());
                }
            }
        }

        /// <summary>
        /// process before action => extract binding information
        /// </summary>
        /// <param name="value"></param>
        private void ProcessBeforeAction(object value)
        {
            var httpContext = value.GetType().GetTypeInfo().GetDeclaredProperty("httpContext")?.GetValue(value) as HttpContext;
            var actionDescriptor = value.GetType().GetTypeInfo().GetDeclaredProperty("actionDescriptor")?.GetValue(value) as ControllerActionDescriptor;
            if (httpContext != null)
            {
                var traceIdentifier = httpContext.TraceIdentifier;

                if (info.TryGetValue(traceIdentifier, out var existing))
                {
                    info.TryUpdate(traceIdentifier,
                        new RequestInfo
                        {
                            ActionName = actionDescriptor.ActionName,
                            ControllerName = actionDescriptor.ControllerName,
                            Start = existing.Start
                        },
                        existing);
                }
            }
        }

        /// <summary>
        /// process request started event
        /// </summary>
        /// <param name="value"></param>
        private void ProcessStartEvent(object value)
        {
            var httpContext = value.GetType().GetTypeInfo().GetDeclaredProperty("HttpContext")?.GetValue(value) as HttpContext;
            if (httpContext != null)
            {
                var traceIdentifier = httpContext.TraceIdentifier;

                info.TryAdd(traceIdentifier, new RequestInfo { Start = DateTime.UtcNow, TraceIdentifier = traceIdentifier });
            }
        }
    }
}
