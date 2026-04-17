using BaseFaq.Tools.Seed.Abstractions;
using BaseFaq.Tools.Seed.Configuration;
using BaseFaq.Tools.Seed.Infrastructure;
using BaseFaq.Common.EntityFramework.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;


namespace BaseFaq.Tools.Seed.Application;

public sealed class SeedRunner(
    IConfiguration configuration,
    IConsoleAdapter console,
    IDbContextFactory dbContextFactory,
    ITenantSeedService tenantSeeder,
    IQnASeedService qnaSeeder,
    IBillingSeedService billingSeeder,
    ICleanupService cleanupService,
    SeedCounts counts)
    : ISeedRunner
{
    public int Run()
    {
        var settings = SeedSettings.From(configuration);
        var tenantBuilder = new NpgsqlConnectionStringBuilder(settings.TenantConnectionString);
        var qnaBuilder = new NpgsqlConnectionStringBuilder(settings.QnAConnectionString);
        var tenantSeedRequest = new TenantSeedRequest(tenantBuilder.ToString(), qnaBuilder.ToString());

        console.WriteLine(
            $"Using TenantDb from appsettings.json: {FormatConnectionInfo(tenantBuilder)}");
        console.WriteLine(
            $"Using QnADb from appsettings.json: {FormatConnectionInfo(qnaBuilder)}");

        var action = PromptAction(console);
        if (action == SeedAction.Exit)
        {
            return 0;
        }

        var seedUserId = Guid.NewGuid();
        var httpContextAccessor = new HttpContextAccessor();

        var tenantSessionService = new SeedSessionService(seedUserId, Guid.Empty);
        var dummyTenantProvider = new StaticTenantConnectionStringProvider(qnaBuilder.ToString());

        using var tenantDb = dbContextFactory.CreateTenantDbContext(
            tenantBuilder.ToString(),
            configuration,
            tenantSessionService,
            dummyTenantProvider,
            httpContextAccessor);

        PostgresDatabaseProvisioner.EnsureDatabaseExists(tenantBuilder.ToString());
        tenantDb.Database.Migrate();

        if (action is SeedAction.CleanTenantOnly or SeedAction.CleanAndSeed)
        {
            cleanupService.CleanTenantDb(tenantDb);
        }

        if (action == SeedAction.CleanTenantOnly)
        {
            return 0;
        }

        if (action == SeedAction.SeedEssentialOnly)
        {
            var essentialSeed = tenantSeeder.EnsureEssentialData(tenantDb, tenantSeedRequest, counts);
            console.WriteLine("Essential seed complete.");
            console.WriteLine("Tenant metadata ensured.");
            console.WriteLine($"Seed tenant id: {essentialSeed.TenantId}");
            return 0;
        }

        if (action is SeedAction.SeedSampleOnly or SeedAction.CleanAndSeed)
        {
            EssentialSeedResult essentialSeed;
            if (action == SeedAction.CleanAndSeed)
            {
                essentialSeed = tenantSeeder.EnsureEssentialData(tenantDb, tenantSeedRequest, counts);
                console.WriteLine("Essential seed complete.");
            }
            else if (!tenantSeeder.HasEssentialData(tenantDb, tenantSeedRequest, counts))
            {
                console.WriteLine(
                    "Essential data is missing. Run 'Seed essential data (tenant metadata)' first.");
                return 1;
            }
            else
            {
                essentialSeed = tenantSeeder.EnsureEssentialData(tenantDb, tenantSeedRequest, counts);
            }

            var seedTenantId = essentialSeed.TenantId;

            var qnaSessionService = new SeedSessionService(seedUserId, seedTenantId);
            var qnaTenantProvider = new StaticTenantConnectionStringProvider(qnaBuilder.ToString());

            using var qnaDb = dbContextFactory.CreateQnADbContext(
                qnaBuilder.ToString(),
                configuration,
                qnaSessionService,
                qnaTenantProvider,
                httpContextAccessor);

            PostgresDatabaseProvisioner.EnsureDatabaseExists(qnaBuilder.ToString());
            qnaDb.Database.Migrate();
            qnaDb.TenantFiltersEnabled = false;
            qnaDb.SoftDeleteFiltersEnabled = false;

            if (action is SeedAction.CleanAndSeed)
            {
                cleanupService.CleanQnADb(qnaDb);
            }

            if (qnaSeeder.HasData(qnaDb) &&
                !Confirm(console, "QnA database already has data. Append seed data?"))
            {
                return 0;
            }

            qnaSeeder.Seed(qnaDb, seedTenantId, counts);

            if (billingSeeder.HasBillingData(tenantDb, seedTenantId) &&
                !Confirm(console, "Billing sample data already exists. Re-seed billing scenarios?"))
            {
                return 0;
            }

            billingSeeder.SeedBillingData(tenantDb, seedTenantId, tenantSeedRequest.QnAConnectionString);
            console.WriteLine("Billing sample data seeded for tenant-001 plus 5 demo billing scenarios.");
            return 0;
        }

        var cleanQnaSessionService = new SeedSessionService(seedUserId, Guid.Empty);
        var cleanQnaTenantProvider = new StaticTenantConnectionStringProvider(qnaBuilder.ToString());

        using var cleanQnaDb = dbContextFactory.CreateQnADbContext(
            qnaBuilder.ToString(),
            configuration,
            cleanQnaSessionService,
            cleanQnaTenantProvider,
            httpContextAccessor);

        PostgresDatabaseProvisioner.EnsureDatabaseExists(qnaBuilder.ToString());
        cleanQnaDb.Database.Migrate();
        cleanQnaDb.TenantFiltersEnabled = false;
        cleanQnaDb.SoftDeleteFiltersEnabled = false;

        cleanupService.CleanQnADb(cleanQnaDb);
        return 0;
    }

    private static string FormatConnectionInfo(NpgsqlConnectionStringBuilder builder)
    {
        return $"Host={builder.Host};Port={builder.Port};Database={builder.Database};Username={builder.Username}";
    }

    private static bool Confirm(IConsoleAdapter console, string message)
    {
        console.Write($"{message} (y/N): ");
        var input = console.ReadLine();
        return string.Equals(input, "y", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(input, "yes", StringComparison.OrdinalIgnoreCase);
    }

    private static SeedAction PromptAction(IConsoleAdapter console)
    {
        console.WriteLine("Select action:");
        console.WriteLine("1) Seed realistic sample QnA data (default)");
        console.WriteLine("2) Seed essential data (tenant metadata)");
        console.WriteLine("3) Clean databases and seed essential + sample QnA data");
        console.WriteLine("4) Clean TenantDb only");
        console.WriteLine("5) Clean QnADb only");
        console.WriteLine("0) Exit");
        console.Write("Choice: ");
        var input = console.ReadLine();
        return input switch
        {
            "2" => SeedAction.SeedEssentialOnly,
            "3" => SeedAction.CleanAndSeed,
            "4" => SeedAction.CleanTenantOnly,
            "5" => SeedAction.CleanQnAOnly,
            "0" => SeedAction.Exit,
            _ => SeedAction.SeedSampleOnly
        };
    }

    private enum SeedAction
    {
        SeedSampleOnly,
        SeedEssentialOnly,
        CleanAndSeed,
        CleanTenantOnly,
        CleanQnAOnly,
        Exit
    }
}
