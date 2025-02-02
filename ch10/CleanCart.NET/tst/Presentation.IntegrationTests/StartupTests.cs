using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Presentation.BSA;

namespace Presentation.IntegrationTests;

public class StartupTests
{
    [Fact]
    public void Application_StartsSuccessfully()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        var builder = Program.CreateHostBuilder(Array.Empty<string>());

        // Build and start the host
        var host = builder.Build();
        host.Start();

        // Act & Assert
        using var testServer = new TestServer(host.Services);
        Assert.NotNull(testServer);
    }
}