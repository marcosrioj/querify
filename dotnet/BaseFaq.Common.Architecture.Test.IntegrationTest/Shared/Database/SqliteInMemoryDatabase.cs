using Microsoft.Data.Sqlite;

namespace BaseFaq.Common.Architecture.Test.IntegrationTest.Shared.Database;

public sealed class SqliteInMemoryDatabase : IDisposable
{
    private const string ApplicationCollation = "Latin1_General_100_CI_AS_SC_UTF8";

    public SqliteInMemoryDatabase(string? databaseName = null)
    {
        DatabaseName = databaseName ?? $"basefaq-tests-{Guid.NewGuid():N}";
        ConnectionString = new SqliteConnectionStringBuilder
        {
            DataSource = DatabaseName,
            Mode = SqliteOpenMode.Memory,
            Cache = SqliteCacheMode.Shared
        }.ToString();

        Connection = new SqliteConnection(ConnectionString);
        Connection.Open();

        Configure(Connection);
    }

    public string DatabaseName { get; }

    public string ConnectionString { get; }

    public SqliteConnection Connection { get; }

    public void Dispose()
    {
        Connection.Dispose();
    }

    private static void Configure(SqliteConnection connection)
    {
        connection.CreateCollation(
            ApplicationCollation,
            static (left, right) => StringComparer.InvariantCultureIgnoreCase.Compare(left, right));

        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA foreign_keys = ON;";
        command.ExecuteNonQuery();
    }
}
