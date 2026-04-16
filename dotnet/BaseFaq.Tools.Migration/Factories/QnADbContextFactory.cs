using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Common.EntityFramework.Tenant.Entities;
using BaseFaq.Models.Common.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb;
using BaseFaq.Tools.Migration.Configuration;
using BaseFaq.Tools.Migration.Services;
using BaseFaq.Tools.Migration.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace BaseFaq.Tools.Migration.Factories;

public sealed class QnADbContextFactory : IDesignTimeDbContextFactory<QnADbContext>
{
    public QnADbContext CreateDbContext(string[] args)
    {
        var configuration = MigrationsConfiguration.Build(SolutionRootLocator.Find());
        var app = ResolveAppEnum(args);
        if (app != AppEnum.QnA)
        {
            throw new InvalidOperationException(
                $"App '{app}' is not supported by {nameof(QnADbContextFactory)}.");
        }

        var tenantDbConnectionString = MigrationsConfiguration.GetTenantDbConnectionString(configuration);
        var designTimeConnectionString = ResolveDesignTimeConnectionString(configuration, tenantDbConnectionString);
        var sessionService = new MigrationsSessionService();
        var tenantConnectionProvider = new NoopTenantConnectionStringProvider();
        var httpContextAccessor = new HttpContextAccessor();

        TenantConnection tenantConnection;
        try
        {
            using var tenantDbContext = new TenantDbContext(
                new DbContextOptionsBuilder<TenantDbContext>()
                    .UseNpgsql(tenantDbConnectionString)
                    .Options,
                sessionService,
                configuration,
                tenantConnectionProvider,
                httpContextAccessor);

            tenantConnection = ResolveCurrentConnection(tenantDbContext);
        }
        catch when (!string.IsNullOrWhiteSpace(designTimeConnectionString))
        {
            tenantConnection = new TenantConnection
            {
                ConnectionString = designTimeConnectionString,
                App = app,
                IsCurrent = true
            };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "Failed to connect to the tenant database while creating the QnA DbContext. " +
                "Make sure the database is running and ConnectionStrings:TenantDb is set correctly, " +
                "or provide ConnectionStrings:QnADb for offline design-time scaffolding.",
                ex);
        }

        var options = new DbContextOptionsBuilder<QnADbContext>()
            .UseNpgsql(tenantConnection.ConnectionString)
            .Options;

        return new QnADbContext(
            options,
            sessionService,
            configuration,
            tenantConnectionProvider,
            httpContextAccessor);
    }

    private static string ResolveDesignTimeConnectionString(
        IConfiguration configuration,
        string tenantDbConnectionString)
    {
        var qnaDbConnectionString = configuration.GetConnectionString("QnADb");
        if (!string.IsNullOrWhiteSpace(qnaDbConnectionString))
        {
            return qnaDbConnectionString;
        }

        return tenantDbConnectionString;
    }

    private static TenantConnection ResolveCurrentConnection(TenantDbContext tenantDbContext)
    {
        try
        {
            return tenantDbContext
                .GetCurrentTenantConnection(AppEnum.QnA)
                .GetAwaiter()
                .GetResult();
        }
        catch
        {
            return tenantDbContext
                .GetCurrentTenantConnection(AppEnum.Faq)
                .GetAwaiter()
                .GetResult();
        }
    }

    private static AppEnum ResolveAppEnum(string[] args)
    {
        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (string.Equals(arg, "--app", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                return ParseAppEnum(args[i + 1]);
            }

            if (arg.StartsWith("--app=", StringComparison.OrdinalIgnoreCase))
            {
                return ParseAppEnum(arg["--app=".Length..]);
            }
        }

        return AppEnum.QnA;
    }

    private static AppEnum ParseAppEnum(string value)
    {
        if (!Enum.TryParse<AppEnum>(value, ignoreCase: true, out var app))
        {
            throw new InvalidOperationException($"Unknown app value '{value}'.");
        }

        return app;
    }
}
