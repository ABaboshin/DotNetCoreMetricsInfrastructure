namespace Metrics
{
    /// <summary>
    /// metric sender
    /// </summary>
    public interface IMetricsSender
    {
        /// <summary>
        /// histogram
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="statName"></param>
        /// <param name="value"></param>
        /// <param name="sampleRate"></param>
        /// <param name="tags"></param>
        void Histogram<T>(string statName, T value, double sampleRate = 1, string[] tags = null);
    }
}
