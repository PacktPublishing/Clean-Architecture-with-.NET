using Application.Interfaces.Auth;
using AutoMapper;
using Infrastructure.Extensions;
using Infrastructure.Startup;
using Presentation.BSA.Auth;
using Presentation.BSA.Extensions;
using Presentation.BSA.Mapping;
using Presentation.BSA.Services;
using StartupOrchestration.NET;

namespace Presentation.BSA;

public class Startup : StartupOrchestrator<AppStartupOrchestrator>
{
    public Startup()
    {
        // Add Presentation Services
        ServiceRegistrationExpressions.Add((services, config) => services.AddAzureB2C(config));
        ServiceRegistrationExpressions.Add((services, config) => services.AddAuthorization(config));
        ServiceRegistrationExpressions.Add((services, config) => services.AddRazorPagesWithAuthorization(config));
        ServiceRegistrationExpressions.Add((services, config) => services.AddCascadingAuthenticationState());
        ServiceRegistrationExpressions.Add((services, config) => services.AddServerSideBlazor(null));
        ServiceRegistrationExpressions.Add((services, config) => services.AddMudBlazorServices(config));

        // Add Services
        ServiceRegistrationExpressions.Add((services, config) => services.AddScoped<ShoppingCartState>());
        ServiceRegistrationExpressions.Add((services, config) => services.AddScoped<IAuthenticationService, BlazorAuthenticationService>());

        // Add AutoMapper Profiles
        ServiceRegistrationExpressions.Add((services, config) => services.AddSingleton<Profile, PresentationMappingProfile>());
    }

    protected override void AddConfigurationProviders(IConfigurationBuilder builder)
    {
        string assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
        string path = Directory.GetParent(assemblyLocation)!.FullName;
        builder.SetBasePath(path);
        builder.AddCoreLayerConfiguration();
        builder.AddPresentationLayerConfiguration();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Configure the HTTP request pipeline.
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        // UseStaticFiles middleware must be placed before UseRouting
        app.UseStaticFiles();
        app.UseRouting();

        // UseAntiforgery, Authentication and Authorization must go between UseRouting and UseEndpoints
        app.UseAntiforgery();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapBlazorHub();
            endpoints.MapFallbackToPage("/_Host");
            endpoints.MapRazorPages();
            endpoints.MapControllers();
        });
    }
}