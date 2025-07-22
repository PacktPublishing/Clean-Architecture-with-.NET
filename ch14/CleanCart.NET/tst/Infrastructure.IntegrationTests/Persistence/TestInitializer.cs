using AutoMapper;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Infrastructure.Extensions;
using Infrastructure.Persistence.EntityFramework;
using Infrastructure.Startup;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using System.Data.Common;

namespace Infrastructure.IntegrationTests.Persistence;

public sealed class TestInitializer : IAsyncLifetime
{
    private const string Database = "master";
    private const string Username = "sa";
    private const string Password = "yourStrong(!)Password";
    private const ushort MsSqlPort = 1433;

    private readonly IContainer _mssqlContainer = new ContainerBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-CU1-ubuntu-20.04")
        .WithPortBinding(MsSqlPort, true)
        .WithEnvironment("ACCEPT_EULA", "Y")
        .WithEnvironment("SQLCMDUSER", Username)
        .WithEnvironment("SQLCMDPASSWORD", Password)
        .WithEnvironment("MSSQL_SA_PASSWORD", Password)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilCommandIsCompleted("/opt/mssql-tools/bin/sqlcmd", "-Q", "SELECT 1;"))
        .Build();

    private DbConnection _dbConnection = default!;
    private Respawner _respawner = default!;

    public IDbContextFactory<CoreDbContext> DbContextFactory = default!;
    public IMapper Mapper { get; private set; } = default!;

    public async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_dbConnection);
    }

    public async Task InitializeAsync()
    {
        await _mssqlContainer.StartAsync();
        await InitializeDbConnection();
        await InitializeServices();
        await InitializeDatabase();
        await InitializeRespawner();
    }

    private async Task InitializeDbConnection()
    {
        _dbConnection = new SqlConnection(GetConnectionString());
        await _dbConnection.OpenAsync();
    }

    private async Task InitializeRespawner()
    {
        _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer,
            SchemasToInclude = ["dbo"]
            // Include/Exclude schemas, tables, and views
        });
    }

    private Task InitializeServices()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().AddCoreLayerConfiguration().Build();
        var appStartupOrchestrator = new AppStartupOrchestrator();
        appStartupOrchestrator.Orchestrate(services, configuration);
        services.Remove(services.SingleOrDefault(service => typeof(DbContextOptions<CoreDbContext>) == service.ServiceType)!);
        services.Remove(services.SingleOrDefault(service => typeof(DbConnection) == service.ServiceType)!);
        services.AddDbContext<CoreDbContext>(options =>
        {
            options.UseSqlServer(GetConnectionString());
        });
        var serviceProvider = services.BuildServiceProvider();
        Mapper = serviceProvider.GetRequiredService<IMapper>();
        DbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<CoreDbContext>>();
        return Task.CompletedTask;
    }

    private async Task InitializeDatabase()
    {
        await using var dbContext = await DbContextFactory.CreateDbContextAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _mssqlContainer.DisposeAsync();
        await _dbConnection.DisposeAsync();
    }

    public string GetConnectionString()
    {
        return $"Server={_mssqlContainer.Hostname},{_mssqlContainer.GetMappedPublicPort(MsSqlPort)};Database={Database};User Id={Username};Password={Password};TrustServerCertificate=True";
    }
}