namespace Infrastructure.Helpers;

/// <summary>
/// Provides utility methods and constants for working with the ASP.NET Core runtime environment.
/// </summary>
public static class AspNetEnvironmentHelper
{
    /// <summary>
    /// The name of the environment variable used to specify the runtime environment for an ASP.NET Core application.
    /// </summary>
    /// <remarks>Common values for this environment variable include <see langword="Development"/>, <see
    /// langword="Staging"/>, and <see langword="Production"/>. This variable is typically used to configure application
    /// behavior based on the current environment.</remarks>
    public const string EnvironmentVariableName = "ASPNETCORE_ENVIRONMENT";

    /// <summary>
    /// Retrieves the value of the required environment variable <c>ASPNETCORE_ENVIRONMENT</c>.
    /// </summary>
    /// <remarks>This method reads the <c>ASPNETCORE_ENVIRONMENT</c> environment variable and returns its
    /// value. If the variable is not set or its value is empty, an <see cref="InvalidOperationException"/> is
    /// thrown.</remarks>
    /// <returns>The value of the <c>ASPNETCORE_ENVIRONMENT</c> environment variable.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the <c>ASPNETCORE_ENVIRONMENT</c> environment variable is not set or its value is empty.</exception>
    public static string GetRequiredEnvironmentName()
    {
        string? environment = Environment.GetEnvironmentVariable(EnvironmentVariableName);
        if (string.IsNullOrEmpty(environment))
        {
            throw new InvalidOperationException($"The environment variable '{EnvironmentVariableName}' is not set.");
        }
        return environment;
    }

    /// <summary>
    /// Determines whether the current environment is set to "Development".
    /// </summary>
    /// <remarks>This method checks the value of the environment variable defined by <see
    /// cref="EnvironmentVariableName"/>  to determine the current environment. Ensure that the environment variable is
    /// properly configured before calling this method.</remarks>
    /// <returns><see langword="true"/> if the environment variable specified by <see cref="EnvironmentVariableName"/>  is set to
    /// "Development"; otherwise, <see langword="false"/>.</returns>
    public static bool IsDevelopment()
    {
        return string.Equals(Environment.GetEnvironmentVariable(EnvironmentVariableName), "Development", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines whether the current environment is set to "Test", ignoring case.
    /// </summary>
    /// <returns><see langword="true"/> if the environment variable is set to "Test" (case-insensitive); otherwise, <see langword="false"/>.</returns>
    public static bool IsTest()
    {
        return string.Equals(Environment.GetEnvironmentVariable(EnvironmentVariableName), "Test", StringComparison.OrdinalIgnoreCase);
    }
}