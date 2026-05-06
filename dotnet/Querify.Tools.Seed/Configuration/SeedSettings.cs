using Microsoft.Extensions.Configuration;

namespace Querify.Tools.Seed.Configuration;

public sealed record SeedSettings(string TenantConnectionString, string QnAConnectionString)
{
    public static SeedSettings From(IConfiguration configuration)
    {
        var tenant = GetRequiredConnectionString(configuration, "TenantDb");
        var qna = GetRequiredConnectionString(configuration, "QnADb");
        return new SeedSettings(tenant, qna);
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
}
