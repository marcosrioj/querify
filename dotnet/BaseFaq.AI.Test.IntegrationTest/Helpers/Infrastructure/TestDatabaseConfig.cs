using Microsoft.Extensions.Configuration;

namespace BaseFaq.AI.Test.IntegrationTest.Helpers.Infrastructure;

public sealed class TestDatabaseConfig
{
    private TestDatabaseConfig(string host, int port, string username, string password, string adminDatabase)
    {
        Host = host;
        Port = port;
        Username = username;
        Password = password;
        AdminDatabase = adminDatabase;
    }

    public string Host { get; }
    public int Port { get; }
    public string Username { get; }
    public string Password { get; }
    public string AdminDatabase { get; }

    public static TestDatabaseConfig Load()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var host = configuration["BaseFaqTestDb:Host"] ?? "localhost";
        var portRaw = configuration["BaseFaqTestDb:Port"] ?? "5432";
        var username = configuration["BaseFaqTestDb:User"] ?? "postgres";
        var password = configuration["BaseFaqTestDb:Password"] ?? "Pass123$";
        var adminDatabase = configuration["BaseFaqTestDb:AdminDatabase"] ?? "postgres";

        if (!int.TryParse(portRaw, out var port))
        {
            throw new InvalidOperationException("BaseFaqTestDb:Port must be a valid integer.");
        }

        return new TestDatabaseConfig(host, port, username, password, adminDatabase);
    }

    public string BuildAdminConnectionString()
    {
        return $"Host={Host};Port={Port};Database={AdminDatabase};Username={Username};Password={Password}";
    }

    public string BuildDatabaseConnectionString(string databaseName)
    {
        return $"Host={Host};Port={Port};Database={databaseName};Username={Username};Password={Password}";
    }
}