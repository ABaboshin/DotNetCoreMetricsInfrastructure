using MassTransit;
using System.Threading.Tasks;

namespace Metrics.UnitTests.MassTransit
{
    public class OkConsumer : IConsumer<OkMessage>
    {
        public Task Consume(ConsumeContext<OkMessage> context)
        {
            return Task.CompletedTask;
        }
    }
}
