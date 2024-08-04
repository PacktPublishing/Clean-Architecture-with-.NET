using Infrastructure.Extensions;
using Presentation.BSA.Extensions;

namespace Presentation.BSA;

public static class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
                webBuilder.ConfigureAppConfiguration((hostingContext, config) =>
                {
                    string assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    string path = Directory.GetParent(assemblyLocation)!.FullName;
                    config.SetBasePath(path);
                    config.AddCoreLayerConfiguration();
                    config.AddPresentationLayerConfiguration();
                });
            });
}