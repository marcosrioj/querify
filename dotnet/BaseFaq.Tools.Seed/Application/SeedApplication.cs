using BaseFaq.Tools.Seed.Abstractions;
using BaseFaq.Tools.Seed.Configuration;
using BaseFaq.Tools.Seed.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace BaseFaq.Tools.Seed.Application;

public sealed class SeedApplication
{
    private readonly ISeedRunner _runner;

    private SeedApplication(ISeedRunner runner)
    {
        _runner = runner;
    }

    public int Run()
    {
        return _runner.Run();
    }

    public static SeedApplication Build()
    {
        var configuration = BuildConfiguration();
        var console = new ConsoleAdapter();
        var dbContextFactory = new DbContextFactory();
        var tenantSeeder = new TenantSeedService();
        var faqSeeder = new FaqSeedService();
        var billingSeeder = new BillingSeedService();
        var cleanupService = new CleanupService();
        var counts = SeedCounts.Default;

        var runner = new SeedRunner(
            configuration,
            console,
            dbContextFactory,
            tenantSeeder,
            faqSeeder,
            billingSeeder,
            cleanupService,
            counts);

        return new SeedApplication(runner);
    }

    private static IConfiguration BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables();

        var baseDirectory = AppContext.BaseDirectory;
        if (!string.Equals(baseDirectory, Directory.GetCurrentDirectory(), StringComparison.OrdinalIgnoreCase))
        {
            builder.AddJsonFile(Path.Combine(baseDirectory, "appsettings.json"), optional: true);
        }

        return builder.Build();
    }
}