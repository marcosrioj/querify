using System.Data;
using Microsoft.EntityFrameworkCore;

namespace Querify.QnA.Worker.Business.Source.Infrastructure;

public static class DbContextTableExtensions
{
    private const string SqliteProviderName = "Microsoft.EntityFrameworkCore.Sqlite";
    private const string NpgsqlProviderName = "Npgsql.EntityFrameworkCore.PostgreSQL";

    public static async Task<bool> TableExistsAsync(
        this DbContext dbContext,
        string tableName,
        CancellationToken cancellationToken = default)
    {
        var connection = dbContext.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;

        if (shouldClose)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            await using var command = connection.CreateCommand();
            var parameter = command.CreateParameter();
            parameter.ParameterName = "tableName";

            if (string.Equals(dbContext.Database.ProviderName, SqliteProviderName, StringComparison.Ordinal))
            {
                command.CommandText = """
                                      select exists(
                                          select 1
                                          from sqlite_master
                                          where type = 'table' and name = @tableName
                                      )
                                      """;
                parameter.Value = tableName;
            }
            else if (string.Equals(dbContext.Database.ProviderName, NpgsqlProviderName, StringComparison.Ordinal))
            {
                command.CommandText = "select to_regclass(@tableName) is not null";
                parameter.Value = $"public.\"{tableName}\"";
            }
            else
            {
                command.CommandText = """
                                      select count(*)
                                      from information_schema.tables
                                      where table_name = @tableName
                                      """;
                parameter.Value = tableName;
            }

            command.Parameters.Add(parameter);

            var result = await command.ExecuteScalarAsync(cancellationToken);
            return result switch
            {
                bool exists => exists,
                long count => count > 0,
                int count => count > 0,
                _ => false
            };
        }
        finally
        {
            if (shouldClose)
            {
                await connection.CloseAsync();
            }
        }
    }
}
