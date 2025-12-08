namespace Infrastructure.Configuration;

public class SqlServerOptions
{
    public string? ConnectionString { get; set; }

    public bool EnableSensitiveDataLogging { get; set; }
}