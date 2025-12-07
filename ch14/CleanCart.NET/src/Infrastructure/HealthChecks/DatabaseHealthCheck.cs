using Infrastructure.Persistence.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Infrastructure.HealthChecks;

/// <summary>
/// Health check to verify that the database is reachable and can accept connections.
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly IDbContextFactory<CoreDbContext> _contextFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseHealthCheck"/> class.
    /// </summary>
    /// <param name="contextFactory">The factory used to create <see cref="CoreDbContext"/> instances.</param>
    public DatabaseHealthCheck(IDbContextFactory<CoreDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var dbContext = await _contextFactory.CreateDbContextAsync(cancellationToken);

            // A lightweight query to verify the DB is reachable
            var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);

            return canConnect
                ? HealthCheckResult.Healthy("Database is reachable.")
                : HealthCheckResult.Unhealthy("Database connection failed.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("An exception occurred while checking database health.", ex);
        }
    }
}