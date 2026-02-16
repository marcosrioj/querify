using BaseFaq.Tools.Seed.Abstractions;
using BaseFaq.Tools.Seed.Configuration;
using BaseFaq.Tools.Seed.Infrastructure;
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
    IFaqSeedService faqSeeder,
    ICleanupService cleanupService,
    SeedCounts counts)
    : ISeedRunner
{
    public int Run()
    {
        var settings = SeedSettings.From(configuration);
        var tenantBuilder = new NpgsqlConnectionStringBuilder(settings.TenantConnectionString);
        var faqBuilder = new NpgsqlConnectionStringBuilder(settings.FaqConnectionString);

        console.WriteLine(
            $"Using TenantDb from appsettings.json: {FormatConnectionInfo(tenantBuilder)}");
        console.WriteLine(
            $"Using FaqDb from appsettings.json: {FormatConnectionInfo(faqBuilder)}");

        var action = PromptAction(console);
        if (action == SeedAction.Exit)
        {
            return 0;
        }

        var seedUserId = Guid.NewGuid();
        var httpContextAccessor = new HttpContextAccessor();

        var tenantSessionService = new SeedSessionService(seedUserId, Guid.Empty);
        var dummyTenantProvider = new StaticTenantConnectionStringProvider(faqBuilder.ToString());

        using var tenantDb = dbContextFactory.CreateTenantDbContext(
            tenantBuilder.ToString(),
            configuration,
            tenantSessionService,
            dummyTenantProvider,
            httpContextAccessor);

        tenantDb.Database.Migrate();

        if (action is SeedAction.CleanOnly or SeedAction.CleanAndSeed)
        {
            cleanupService.CleanTenantDb(tenantDb);
        }

        if (action == SeedAction.SeedEssentialOnly)
        {
            var iaAgentUserId = tenantSeeder.EnsureEssentialData(tenantDb);
            console.WriteLine("Essential seed complete.");
            console.WriteLine("AI providers ensured.");
            console.WriteLine($"IA Agent user id: {iaAgentUserId}");
            console.WriteLine("Set this value in AI API appsettings: Ai:UserId");
            return 0;
        }

        if (action is SeedAction.SeedDummyOnly or SeedAction.CleanAndSeed)
        {
            if (action == SeedAction.CleanAndSeed)
            {
                tenantSeeder.EnsureEssentialData(tenantDb);
                console.WriteLine("Essential seed complete.");
            }
            else if (!tenantSeeder.HasEssentialData(tenantDb))
            {
                console.WriteLine(
                    "Essential data is missing. Run 'Seed essential data (AI providers + IA Agent user)' first.");
                return 1;
            }

            if (tenantSeeder.HasData(tenantDb) &&
                !Confirm(console, "Tenant database already has data. Append seed data?"))
            {
                return 0;
            }

            var seedTenantId = tenantSeeder.SeedDummyData(
                tenantDb,
                new TenantSeedRequest(tenantBuilder.ToString(), faqBuilder.ToString()),
                counts);

            var faqSessionService = new SeedSessionService(seedUserId, seedTenantId);
            var faqTenantProvider = new StaticTenantConnectionStringProvider(faqBuilder.ToString());

            using var faqDb = dbContextFactory.CreateFaqDbContext(
                faqBuilder.ToString(),
                configuration,
                faqSessionService,
                faqTenantProvider,
                httpContextAccessor);

            faqDb.Database.Migrate();
            faqDb.TenantFiltersEnabled = false;
            faqDb.SoftDeleteFiltersEnabled = false;

            if (action is SeedAction.CleanAndSeed)
            {
                cleanupService.CleanFaqDb(faqDb);
            }

            if (faqSeeder.HasData(faqDb) &&
                !Confirm(console, "FAQ database already has data. Append seed data?"))
            {
                return 0;
            }

            faqSeeder.Seed(faqDb, seedTenantId, counts);
            return 0;
        }

        var cleanFaqSessionService = new SeedSessionService(seedUserId, Guid.Empty);
        var cleanFaqTenantProvider = new StaticTenantConnectionStringProvider(faqBuilder.ToString());

        using var cleanFaqDb = dbContextFactory.CreateFaqDbContext(
            faqBuilder.ToString(),
            configuration,
            cleanFaqSessionService,
            cleanFaqTenantProvider,
            httpContextAccessor);

        cleanFaqDb.Database.Migrate();
        cleanFaqDb.TenantFiltersEnabled = false;
        cleanFaqDb.SoftDeleteFiltersEnabled = false;

        cleanupService.CleanFaqDb(cleanFaqDb);
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
        console.WriteLine("1) Seed dummy data (default)");
        console.WriteLine("2) Seed essential data (AI providers + IA Agent user)");
        console.WriteLine("3) Clean databases and seed essential + dummy data");
        console.WriteLine("4) Clean databases only");
        console.WriteLine("0) Exit");
        console.Write("Choice: ");
        var input = console.ReadLine();
        return input switch
        {
            "2" => SeedAction.SeedEssentialOnly,
            "3" => SeedAction.CleanAndSeed,
            "4" => SeedAction.CleanOnly,
            "0" => SeedAction.Exit,
            _ => SeedAction.SeedDummyOnly
        };
    }

    private enum SeedAction
    {
        SeedDummyOnly,
        SeedEssentialOnly,
        CleanAndSeed,
        CleanOnly,
        Exit
    }
}