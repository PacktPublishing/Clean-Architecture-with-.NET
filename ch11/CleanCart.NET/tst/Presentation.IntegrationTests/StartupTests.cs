using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Presentation.BSA;

namespace Presentation.IntegrationTests;

public class StartupTests
{
    [Fact]
    public void Application_StartsSuccessfully()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        var builder = Program.CreateHostBuilder(Array.Empty<string>());

        var host = builder.Build();
        host.Start();

        using var testServer = new TestServer(host.Services);
        Assert.NotNull(testServer);
    }
}