using BaseFaq.Common.EntityFramework.Core;
using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.QnA.Common.Persistence.QnADb.DbContext;
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
    IQnASeedService qnaSeeder,
    IBigDataSeedService bigDataSeeder,
    IBillingSeedService billingSeeder,
    ICleanupService cleanupService,
    SeedCounts counts)
    : ISeedRunner
{
    public int Run()
    {
        var settings = SeedSettings.From(configuration);
        var bigDataSettings = BigDataSeedSettings.From(configuration);
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

        using var tenantDb = CreateTenantDbContext(
            tenantBuilder.ToString(),
            qnaBuilder.ToString(),
            seedUserId,
            httpContextAccessor);

        switch (action)
        {
            case SeedAction.SeedEssentialOnly:
                EnsureEssentialData(tenantDb, tenantSeedRequest);
                return 0;

            case SeedAction.SeedRealistic:
                return SeedRealistic(tenantDb, tenantSeedRequest, qnaBuilder.ToString(), seedUserId, httpContextAccessor);

            case SeedAction.CleanAndSeedRealistic:
                cleanupService.CleanTenantDb(tenantDb);
                return SeedRealistic(
                    tenantDb,
                    tenantSeedRequest,
                    qnaBuilder.ToString(),
                    seedUserId,
                    httpContextAccessor,
                    cleanQnADb: true);

            case SeedAction.SeedBigData:
                return SeedBigData(tenantDb, tenantSeedRequest, qnaBuilder.ToString(), seedUserId, httpContextAccessor, bigDataSettings);

            case SeedAction.CleanBigDataOnly:
                using (var qnaDb = CreateQnADbContext(
                           qnaBuilder.ToString(),
                           seedUserId,
                           Guid.Empty,
                           httpContextAccessor))
                {
                    cleanupService.CleanBigDataQnADb(qnaDb);
                }

                console.WriteLine("Seed Big Data rows cleaned from QnADb.");
                return 0;

            case SeedAction.CleanTenantOnly:
                cleanupService.CleanTenantDb(tenantDb);
                console.WriteLine("TenantDb cleaned.");
                return 0;

            case SeedAction.CleanQnAOnly:
                using (var qnaDb = CreateQnADbContext(
                           qnaBuilder.ToString(),
                           seedUserId,
                           Guid.Empty,
                           httpContextAccessor))
                {
                    cleanupService.CleanQnADb(qnaDb);
                }

                console.WriteLine("QnADb cleaned.");
                return 0;

            case SeedAction.CleanAllOnly:
                cleanupService.CleanTenantDb(tenantDb);
                using (var qnaDb = CreateQnADbContext(
                           qnaBuilder.ToString(),
                           seedUserId,
                           Guid.Empty,
                           httpContextAccessor))
                {
                    cleanupService.CleanQnADb(qnaDb);
                }

                console.WriteLine("TenantDb and QnADb cleaned.");
                return 0;

            case SeedAction.Exit:
            default:
                return 0;
        }
    }

    private TenantDbContext CreateTenantDbContext(
        string tenantConnectionString,
        string qnaConnectionString,
        Guid seedUserId,
        IHttpContextAccessor httpContextAccessor)
    {
        var tenantSessionService = new SeedSessionService(seedUserId, Guid.Empty);
        var dummyTenantProvider = new StaticTenantConnectionStringProvider(qnaConnectionString);

        var tenantDb = dbContextFactory.CreateTenantDbContext(
            tenantConnectionString,
            configuration,
            tenantSessionService,
            dummyTenantProvider,
            httpContextAccessor);

        PostgresDatabaseProvisioner.EnsureDatabaseExists(tenantConnectionString);
        tenantDb.Database.Migrate();

        return tenantDb;
    }

    private QnADbContext CreateQnADbContext(
        string qnaConnectionString,
        Guid seedUserId,
        Guid seedTenantId,
        IHttpContextAccessor httpContextAccessor)
    {
        var qnaSessionService = new SeedSessionService(seedUserId, seedTenantId);
        var qnaTenantProvider = new StaticTenantConnectionStringProvider(qnaConnectionString);

        var qnaDb = dbContextFactory.CreateQnADbContext(
            qnaConnectionString,
            configuration,
            qnaSessionService,
            qnaTenantProvider,
            httpContextAccessor);

        PostgresDatabaseProvisioner.EnsureDatabaseExists(qnaConnectionString);
        qnaDb.Database.Migrate();
        qnaDb.TenantFiltersEnabled = false;
        qnaDb.SoftDeleteFiltersEnabled = false;

        return qnaDb;
    }

    private EssentialSeedResult EnsureEssentialData(TenantDbContext tenantDb, TenantSeedRequest tenantSeedRequest)
    {
        var essentialSeed = tenantSeeder.EnsureEssentialData(tenantDb, tenantSeedRequest, counts);
        console.WriteLine("Essential seed complete.");
        console.WriteLine("Tenant metadata ensured.");
        console.WriteLine($"Seed tenant id: {essentialSeed.TenantId}");
        return essentialSeed;
    }

    private int SeedRealistic(
        TenantDbContext tenantDb,
        TenantSeedRequest tenantSeedRequest,
        string qnaConnectionString,
        Guid seedUserId,
        IHttpContextAccessor httpContextAccessor,
        bool cleanQnADb = false)
    {
        var essentialSeed = EnsureEssentialData(tenantDb, tenantSeedRequest);

        using var qnaDb = CreateQnADbContext(
            qnaConnectionString,
            seedUserId,
            essentialSeed.TenantId,
            httpContextAccessor);

        if (cleanQnADb)
        {
            cleanupService.CleanQnADb(qnaDb);
        }

        if (qnaSeeder.HasData(qnaDb) &&
            !Confirm(console, "QnA database already has data. Append realistic seed data?"))
        {
            return 0;
        }

        qnaSeeder.Seed(qnaDb, essentialSeed.TenantId, counts);
        console.WriteLine("Realistic QnA seed complete.");

        if (billingSeeder.HasBillingData(tenantDb, essentialSeed.TenantId) &&
            !Confirm(console, "Billing seed data already exists. Re-seed billing scenarios?"))
        {
            return 0;
        }

        billingSeeder.SeedBillingData(tenantDb, essentialSeed.TenantId, tenantSeedRequest.QnAConnectionString);
        console.WriteLine("Billing seed data seeded for tenant-001 plus 5 demo billing scenarios.");
        return 0;
    }

    private int SeedBigData(
        TenantDbContext tenantDb,
        TenantSeedRequest tenantSeedRequest,
        string qnaConnectionString,
        Guid seedUserId,
        IHttpContextAccessor httpContextAccessor,
        BigDataSeedSettings bigDataSettings)
    {
        var essentialSeed = EnsureEssentialData(tenantDb, tenantSeedRequest);

        using var qnaDb = CreateQnADbContext(
            qnaConnectionString,
            seedUserId,
            essentialSeed.TenantId,
            httpContextAccessor);

        if (bigDataSeeder.HasData(qnaDb))
        {
            if (!Confirm(console, "Seed Big Data already exists. Clean and re-seed it?"))
            {
                return 0;
            }

            cleanupService.CleanBigDataQnADb(qnaDb);
            console.WriteLine("Existing Seed Big Data rows cleaned.");
        }

        console.WriteLine(
            $"Seeding Big Data: {bigDataSettings.QuestionCount:N0} questions, {bigDataSettings.AnswerCount:N0} answers, {bigDataSettings.ActivityCount:N0} interactions, about {bigDataSettings.EstimatedRowCount:N0} total rows.");
        bigDataSeeder.Seed(qnaDb, essentialSeed.TenantId, bigDataSettings);
        console.WriteLine("Seed Big Data complete.");
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
        console.WriteLine("1) Seed Realistic QnA data (default)");
        console.WriteLine("2) Seed essential data (tenant metadata)");
        console.WriteLine("3) Seed Big Data (performance)");
        console.WriteLine("4) Clean databases and seed essential + realistic QnA data");
        console.WriteLine("5) Clean Seed Big Data only");
        console.WriteLine("6) Clean TenantDb only");
        console.WriteLine("7) Clean QnADb only");
        console.WriteLine("8) Clean TenantDb + QnADb");
        console.WriteLine("0) Exit");
        console.Write("Choice: ");
        var input = console.ReadLine();
        return input switch
        {
            "2" => SeedAction.SeedEssentialOnly,
            "3" => SeedAction.SeedBigData,
            "4" => SeedAction.CleanAndSeedRealistic,
            "5" => SeedAction.CleanBigDataOnly,
            "6" => SeedAction.CleanTenantOnly,
            "7" => SeedAction.CleanQnAOnly,
            "8" => SeedAction.CleanAllOnly,
            "0" => SeedAction.Exit,
            _ => SeedAction.SeedRealistic
        };
    }

    private enum SeedAction
    {
        SeedRealistic,
        SeedEssentialOnly,
        SeedBigData,
        CleanAndSeedRealistic,
        CleanBigDataOnly,
        CleanTenantOnly,
        CleanQnAOnly,
        CleanAllOnly,
        Exit
    }
}
