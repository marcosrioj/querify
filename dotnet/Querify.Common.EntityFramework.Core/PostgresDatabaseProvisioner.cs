using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Npgsql;
using System.Net;

namespace Querify.Common.EntityFramework.Core;

public static class PostgresDatabaseProvisioner
{
    public static void EnsureDatabaseExists(string connectionString)
    {
        var targetBuilder = new NpgsqlConnectionStringBuilder(connectionString);
        if (string.IsNullOrWhiteSpace(targetBuilder.Database))
        {
            throw new ApiErrorException(
                "The target PostgreSQL connection string must include a database name.",
                (int)HttpStatusCode.InternalServerError);
        }

        var targetDatabase = targetBuilder.Database;
        var adminBuilder = new NpgsqlConnectionStringBuilder(connectionString)
        {
            Database = "postgres",
            Pooling = false
        };

        using var connection = new NpgsqlConnection(adminBuilder.ConnectionString);
        connection.Open();

        using var existsCommand = new NpgsqlCommand(
            "SELECT 1 FROM pg_database WHERE datname = @databaseName;",
            connection);
        existsCommand.Parameters.AddWithValue("databaseName", targetDatabase);

        if (existsCommand.ExecuteScalar() is not null)
        {
            return;
        }

        var quotedDatabaseName = QuoteIdentifier(targetDatabase);
        using var createCommand = new NpgsqlCommand($"CREATE DATABASE {quotedDatabaseName};", connection);
        createCommand.ExecuteNonQuery();
    }

    private static string QuoteIdentifier(string identifier)
    {
        return "\"" + identifier.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
    }
}
