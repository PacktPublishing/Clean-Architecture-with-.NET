using Application.Interfaces.Services.Telemetry;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Infrastructure.MediatR;

/// <summary>
/// A MediatR pipeline behavior that handles both metrics and logging for requests and responses.
/// Tracks execution times, logs request/response details conditionally, and captures metrics for monitoring.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public class MetricsAndLoggingPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<MetricsAndLoggingPipelineBehavior<TRequest, TResponse>> _logger;
    private readonly IMetricsService _metricsService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetricsAndLoggingPipelineBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="logger">The logger used for structured logging.</param>
    /// <param name="metricsService">The metrics service used for tracking metrics.</param>
    public MetricsAndLoggingPipelineBehavior(ILogger<MetricsAndLoggingPipelineBehavior<TRequest, TResponse>> logger, IMetricsService metricsService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _metricsService = metricsService ?? throw new ArgumentNullException(nameof(metricsService));
    }

    /// <summary>
    /// Handles the request, conditionally logs details, and tracks metrics such as execution time and success/failure counts.
    /// </summary>
    /// <param name="request">The MediatR request being processed.</param>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The response from the request handler.</returns>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = GetFriendlyName(typeof(TRequest));
        var isDebugMode = _logger.IsEnabled(LogLevel.Debug);

        if (isDebugMode)
        {
            _logger.LogDebug("Handling {RequestName} with request payload: {Request}", requestName, request);
        }
        else
        {
            _logger.LogInformation("Handling {RequestName}", requestName);
        }

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next(cancellationToken);
            stopwatch.Stop();

            if (isDebugMode)
            {
                _logger.LogDebug(
                    "Handled {RequestName} successfully in {ElapsedMilliseconds}ms with response payload: {Response}",
                    requestName,
                    stopwatch.ElapsedMilliseconds,
                    response
                );
            }
            else
            {
                _logger.LogInformation("Handled {RequestName} successfully in {ElapsedMilliseconds}ms", requestName, stopwatch.ElapsedMilliseconds);
            }

            if (stopwatch.ElapsedMilliseconds > 500)
            {
                _logger.LogWarning("Request {RequestName} took too long to process: {ElapsedMilliseconds}ms", requestName, stopwatch.ElapsedMilliseconds);
            }

            // Record metrics
            _metricsService.RecordHistogram(name: "mediator_request_latency", value: stopwatch.Elapsed.TotalMilliseconds, labels: ("RequestType", requestName));

            _metricsService.IncrementCounter(name: "mediator_request_total", description: "Total number of MediatR requests processed", labels: ("RequestType", requestName));

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(ex, "Error handling {RequestName} after {ElapsedMilliseconds}ms", requestName, stopwatch.ElapsedMilliseconds);

            // Record failure metrics
            _metricsService.IncrementCounter(name: "mediator_request_failures", description: "Number of failed MediatR requests", labels: ("RequestType", requestName));

            throw;
        }
    }

    /// <summary>
    /// Gets the name of a type, handling generic types gracefully by including their type arguments.
    /// </summary>
    /// <param name="type">The type to get the name for.</param>
    /// <returns>A string representation of the type name, including generic type arguments if applicable.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the provided <paramref name="type"/> is null.</exception>
    public string GetFriendlyName(Type type)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));

        if (!type.IsGenericType)
            return type.Name;

        // Format generic type name, including arguments
        var genericTypeName = type.Name.Substring(0, type.Name.IndexOf('`'));
        var genericArguments = type.GetGenericArguments();
        var argumentNames = string.Join(", ", genericArguments.Select(GetFriendlyName));

        return $"{genericTypeName}<{argumentNames}>";
    }
}