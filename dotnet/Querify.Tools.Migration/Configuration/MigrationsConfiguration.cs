using Microsoft.Extensions.Configuration;

namespace Querify.Tools.Migration.Configuration;

internal static class MigrationsConfiguration
{
    public static IConfiguration Build(string? solutionRoot = null)
    {
        var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                              ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true);

        if (!string.IsNullOrWhiteSpace(environmentName))
        {
            builder.AddJsonFile($"appsettings.{environmentName}.json", optional: true);
        }

        var baseDirectory = AppContext.BaseDirectory;
        if (!string.Equals(baseDirectory, Directory.GetCurrentDirectory(), StringComparison.OrdinalIgnoreCase))
        {
            builder.AddJsonFile(Path.Combine(baseDirectory, "appsettings.json"), optional: true);

            if (!string.IsNullOrWhiteSpace(environmentName))
            {
                builder.AddJsonFile(Path.Combine(baseDirectory, $"appsettings.{environmentName}.json"),
                    optional: true);
            }
        }

        builder.AddEnvironmentVariables();

        if (!string.IsNullOrWhiteSpace(solutionRoot))
        {
            builder.AddJsonFile(
                Path.Combine(solutionRoot, "dotnet", "Querify.Tools.Seed", "appsettings.json"),
                optional: true);

            if (!string.IsNullOrWhiteSpace(environmentName))
            {
                builder.AddJsonFile(
                    Path.Combine(solutionRoot, "dotnet", "Querify.Tools.Seed",
                        $"appsettings.{environmentName}.json"),
                    optional: true);
            }

            builder.AddJsonFile(
                Path.Combine(solutionRoot, "dotnet", "Querify.Tenant.BackOffice.Api", "appsettings.json"),
                optional: true);

            if (!string.IsNullOrWhiteSpace(environmentName))
            {
                builder.AddJsonFile(
                    Path.Combine(solutionRoot, "dotnet", "Querify.Tenant.BackOffice.Api",
                        $"appsettings.{environmentName}.json"),
                    optional: true);
            }
        }

        return builder.Build();
    }

    public static string GetTenantDbConnectionString(IConfiguration configuration)
    {
        var fromConfig = configuration.GetConnectionString("TenantDb");
        if (string.IsNullOrWhiteSpace(fromConfig))
        {
            fromConfig = configuration.GetConnectionString("DefaultConnection");
        }

        if (string.IsNullOrWhiteSpace(fromConfig))
        {
            throw new InvalidOperationException(
                "Missing tenant database connection string. Set ConnectionStrings:TenantDb.");
        }

        return fromConfig;
    }
}
