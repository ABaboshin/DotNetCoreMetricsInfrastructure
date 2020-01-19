using GreenPipes;
using MassTransit;
using System.Collections.Generic;
using System.Linq;

namespace Metrics.Extensions.MassTransit
{
    /// <summary>
    /// An MassTransit extension to track execution times and erros
    /// and send diagnostic events
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TrackConsumingSpecification<T> : IPipeSpecification<T>
        where T: class, ConsumeContext
    {
        public void Apply(IPipeBuilder<T> builder)
        {
            builder.AddFilter(new TrackConsumingFilter<T>());
        }

        public IEnumerable<ValidationResult> Validate()
        {
            return Enumerable.Empty<ValidationResult>();
        }
    }
}