using Application.Interfaces.Services.Telemetry;
using System.Diagnostics.Metrics;

namespace Infrastructure.Services;

/// <summary>
/// Implements the <see cref="IMetricsService"/> interface to provide metric tracking functionality,
/// including incrementing counters and recording histogram values using the provided <see cref="Meter"/>.
/// </summary>
public class MetricsService(Meter meter) : IMetricsService
{
    /// <inheritdoc />
    public void IncrementCounter(string name, string description, params (string Key, string Value)[] labels)
    {
        var counter = meter.CreateCounter<long>(name, description: description);
        counter.Add(1, CreateLabelSet(labels));
    }

    /// <inheritdoc />
    public void RecordHistogram(string name, double value, params (string Key, string Value)[] labels)
    {
        var histogram = meter.CreateHistogram<double>(name);
        histogram.Record(value, CreateLabelSet(labels));
    }

    /// <summary>
    /// Creates a set of labels from an array of key-value pairs.
    /// </summary>
    /// <param name="labels">The key-value pairs representing labels.</param>
    /// <returns>An array of <see cref="KeyValuePair{TKey, TValue}"/> objects representing the labels.</returns>
    private KeyValuePair<string, object?>[] CreateLabelSet((string Key, string Value)[] labels)
    {
        return labels.Select(label => new KeyValuePair<string, object?>(label.Key, label.Value)).ToArray();
    }
}