using GreenPipes;
using MassTransit;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

namespace Metrics.Extensions.MassTransit
{
    /// <summary>
    /// An MassTransit extension to track execution times and erros
    /// and send diagnostic events
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class TrackConsumingFilter<T> : IFilter<T> where T : class, ConsumeContext
    {
        public void Probe(ProbeContext context)
        {
            context.CreateFilterScope("TrackConsumingFilter");
        }

        public async Task Send(T context, IPipe<T> next)
        {
            var source = new DiagnosticListener(DiagnosticListenerUtil.ExectionEventName);

            var message = context.GetType().GetTypeInfo().GetDeclaredProperty("Message")?.GetValue(context);
            var messageId = context.GetType().GetTypeInfo().GetDeclaredProperty("MassTransit.MessageContext.MessageId")?.GetValue(context);

            var activity = new Activity($"Custom Consuming {message.GetType().FullName}")
                .AddTag("messageId", messageId.ToString());

            source.StartActivity(activity, new { context });

            try
            {
                await next.Send(context);
                source.StopActivity(activity, new { context, success = true });
            }
            catch (Exception exception)
            {
                source.StopActivity(activity, new { context, success = false, exception });
                throw;
            }
        }
    }
}