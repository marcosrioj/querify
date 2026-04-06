using Microsoft.Extensions.Configuration;

namespace BaseFaq.Tools.Seed.Configuration;

public sealed record SeedSettings(string TenantConnectionString, string FaqConnectionString)
{
    public static SeedSettings From(IConfiguration configuration)
    {
        var tenant = GetRequiredConnectionString(configuration, "TenantDb");
        var faq = GetRequiredConnectionString(configuration, "FaqDb");
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
}