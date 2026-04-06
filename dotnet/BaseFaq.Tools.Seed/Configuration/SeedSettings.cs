using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Net;
using System.Net.Sockets;

namespace BaseFaq.Tools.Seed.Configuration;

public sealed record SeedSettings(string TenantConnectionString, string FaqConnectionString)
{
    public static SeedSettings From(IConfiguration configuration)
    {
        var tenant = NormalizeConnectionString(GetRequiredConnectionString(configuration, "TenantDb"));
        var faq = NormalizeConnectionString(GetRequiredConnectionString(configuration, "FaqDb"));
        return new SeedSettings(tenant, faq);
    }

    private static string GetRequiredConnectionString(IConfiguration configuration, string name)
    {
        var value = configuration.GetConnectionString(name);
        if (!string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        throw new InvalidOperationException($"Missing connection string '{name}'. Set ConnectionStrings:{name}.");
    }

    private static string NormalizeConnectionString(string connectionString)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        if (string.IsNullOrWhiteSpace(builder.Host) ||
            !string.Equals(builder.Host, "host.docker.internal", StringComparison.OrdinalIgnoreCase))
        {
            return connectionString;
        }

        if (HostResolves(builder.Host))
        {
            return connectionString;
        }

        builder.Host = "127.0.0.1";
        return builder.ToString();
    }

    private static bool HostResolves(string host)
    {
        try
        {
            return Dns.GetHostAddresses(host).Length > 0;
        }
        catch (SocketException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
