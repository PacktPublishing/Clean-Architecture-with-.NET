using Infrastructure.Extensions;
using Presentation.BSA;
using Presentation.Common.Extensions;
using System.Reflection;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------
// Configuration
// ------------------------------------------------------------

// Ensure configuration is resolved relative to the entry assembly
string assemblyLocation = Assembly.GetExecutingAssembly().Location;
string basePath = Directory.GetParent(assemblyLocation)!.FullName;

builder.Configuration
    .SetBasePath(basePath)
    .AddCoreLayerConfiguration()
    .AddPresentationLayerConfiguration();

// ------------------------------------------------------------
// Logging with Serilog
// ------------------------------------------------------------

builder.Host.UseSerilogFromConfiguration();

// ------------------------------------------------------------
// Services
// ------------------------------------------------------------

// Delegate all service registration
var serviceComposition = new PresentationServiceComposition(builder.Configuration);
serviceComposition.ConfigureServices(builder.Services);

var app = builder.Build();

// ------------------------------------------------------------
// Middleware Pipeline
// ------------------------------------------------------------

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Static files are handled before endpoint execution
app.UseStaticFiles();

// Security middleware
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();

// ------------------------------------------------------------
// Endpoints
// ------------------------------------------------------------

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.MapRazorPages();
// Controllers are required for Microsoft.Identity.Web.UI endpoints such as:
// - /MicrosoftIdentity/Account/SignIn
// - /MicrosoftIdentity/Account/SignedOut
app.MapControllers();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
    AllowCachingResponses = false
});

await app.RunAsync();
