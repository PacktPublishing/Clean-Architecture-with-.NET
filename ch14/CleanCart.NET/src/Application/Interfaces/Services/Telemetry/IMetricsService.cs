namespace Application.Interfaces.Services.Telemetry;

/// <summary>
/// Defines methods for tracking and recording metrics in an application.
/// </summary>
public interface IMetricsService
{
    /// <summary>
    /// Increments a counter by 1 for the given metric name and description, with optional labels.
    /// </summary>
    /// <param name="name">The name of the counter metric.</param>
    /// <param name="description">A description of the counter metric.</param>
    /// <param name="labels">An array of key-value pairs to use as labels for the metric.</param>
    void IncrementCounter(string name, string description, params (string Key, string Value)[] labels);

    /// <summary>
    /// Records a value in a histogram for the given metric name, with optional labels.
    /// </summary>
    /// <param name="name">The name of the histogram metric.</param>
    /// <param name="value">The value to record in the histogram.</param>
    /// <param name="labels">An array of key-value pairs to use as labels for the metric.</param>
    void RecordHistogram(string name, double value, params (string Key, string Value)[] labels);
}