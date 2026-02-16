using Microsoft.AspNetCore.Mvc.Testing;

namespace Presentation.IntegrationTests;

public class StartupTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory = factory.WithWebHostBuilder(_ => { });

    [Fact]
    public async Task Application_StartsSuccessfully()
    {
        // Ensure test environment is set to Development for this test
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/");

        Assert.True(response.IsSuccessStatusCode);
    }
}