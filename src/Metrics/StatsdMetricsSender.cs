using StatsdClient;

namespace Metrics
{
    /// <summary>
    /// send metrics to statsd
    /// </summary>
    internal class StatsdMetricsSender : IMetricsSender
    {
        public void Histogram<T>(string statName, T value, double sampleRate = 1, string[] tags = null)
        {
            DogStatsd.Histogram(statName,
                        value,
                        sampleRate,
                        tags);
        }
    }
}
