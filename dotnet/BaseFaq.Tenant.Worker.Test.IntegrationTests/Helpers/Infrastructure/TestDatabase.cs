using Npgsql;

namespace BaseFaq.Tenant.Worker.Test.IntegrationTests.Helpers.Infrastructure;

public sealed class TestDatabase : IDisposable
{
    private TestDatabase(string databaseName, string adminConnectionString, string connectionString)
    {
        DatabaseName = databaseName;
        AdminConnectionString = adminConnectionString;
        ConnectionString = connectionString;
    }

    public string DatabaseName { get; }
    public string AdminConnectionString { get; }
    public string ConnectionString { get; }

    public static TestDatabase Create()
    {
        var config = TestDatabaseConfig.Load();
        var databaseName = $"bf_tenant_test_{Guid.NewGuid():N}";
        var adminConnectionString = config.BuildAdminConnectionString();

        using (var admin = new NpgsqlConnection(adminConnectionString))
        {
            admin.Open();
            using var cmd = admin.CreateCommand();
            cmd.CommandText = $"CREATE DATABASE \"{databaseName}\"";
            cmd.ExecuteNonQuery();
        }

        var connectionString = config.BuildDatabaseConnectionString(databaseName);
        return new TestDatabase(databaseName, adminConnectionString, connectionString);
    }

    public static void DropDatabase(string adminConnectionString, string databaseName)
    {
        using var admin = new NpgsqlConnection(adminConnectionString);
        admin.Open();

        using (var terminate = admin.CreateCommand())
        {
            terminate.CommandText =
                "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = @db AND pid <> pg_backend_pid();";
            terminate.Parameters.AddWithValue("db", databaseName);
            terminate.ExecuteNonQuery();
        }

        using (var drop = admin.CreateCommand())
        {
            drop.CommandText = $"DROP DATABASE IF EXISTS \"{databaseName}\"";
            drop.ExecuteNonQuery();
        }
    }

    public void Dispose()
    {
        DropDatabase(AdminConnectionString, DatabaseName);
    }
}
