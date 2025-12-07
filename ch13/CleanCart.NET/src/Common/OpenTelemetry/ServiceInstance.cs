namespace Common.OpenTelemetry;

/// <summary>
/// Provides a unique identifier for the current instance of the service,
/// primarily used for telemetry and distributed tracing purposes.
/// </summary>
/// <remarks>
/// The service instance ID is generated once when the application starts and remains constant
/// throughout the application's lifecycle. It can be used to distinguish between different
/// service instances in logs and telemetry data.
/// </remarks>
public static class ServiceInstance
{
    /// <summary>
    /// The unique identifier for the current service instance.
    /// </summary>
    /// <remarks>
    /// This ID is generated as a GUID when the application starts and remains static.
    /// It is useful for associating logs and telemetry data with a specific instance of the service.
    /// </remarks>
    public static readonly string InstanceId = Guid.NewGuid().ToString();

    /// <summary>
    /// Retrieves the unique service instance identifier.
    /// </summary>
    /// <returns>The unique identifier for the current service instance.</returns>
    /// <remarks>
    /// This method is provided as an alternative to directly accessing <see cref="InstanceId"/>
    /// and may be useful in scenarios requiring dynamic access to the ID.
    /// </remarks>
    public static string GetInstanceId() => InstanceId;
}