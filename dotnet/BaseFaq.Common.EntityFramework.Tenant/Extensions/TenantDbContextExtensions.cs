using System.Data;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Common.EntityFramework.Tenant.Extensions;

public static class TenantDbContextExtensions
{
    public static async Task<bool> TableExistsAsync(
        this TenantDbContext dbContext,
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
            command.CommandText = "select to_regclass(@tableName) is not null";

            var parameter = command.CreateParameter();
            parameter.ParameterName = "tableName";
            parameter.Value = $"public.{tableName}";
            command.Parameters.Add(parameter);

            var result = await command.ExecuteScalarAsync(cancellationToken);
            return result is bool exists && exists;
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
