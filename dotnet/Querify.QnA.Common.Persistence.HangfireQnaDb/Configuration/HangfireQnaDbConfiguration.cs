using Microsoft.Extensions.Configuration;

namespace Querify.QnA.Common.Persistence.HangfireQnaDb.Configuration;

public static class HangfireQnaDbConfiguration
{
    public const string ConnectionStringName = "HangfireQnaDb";
    public const string LegacyHangFireConnectionPath = "HangFire:ConnectionString";
    public const string DefaultSchemaName = "hangfire";

    public static string GetConnectionString(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName);
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            return connectionString;
        }

        connectionString = configuration[LegacyHangFireConnectionPath];
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            return connectionString;
        }

        throw new InvalidOperationException(
            $"Missing connection string '{ConnectionStringName}'. Configure ConnectionStrings:{ConnectionStringName} for QnA Hangfire persistence.");
    }
}
