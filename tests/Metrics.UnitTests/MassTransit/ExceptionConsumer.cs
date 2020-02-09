using MassTransit;
using System;
using System.Threading.Tasks;

namespace Metrics.UnitTests.MassTransit
{
    public class ExceptionConsumer : IConsumer<ExceptionMessage>
    {
        public Task Consume(ConsumeContext<ExceptionMessage> context)
        {
            throw new NotImplementedException();
        }
    }
}
