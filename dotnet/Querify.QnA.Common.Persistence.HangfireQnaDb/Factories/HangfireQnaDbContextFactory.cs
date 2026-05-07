using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Querify.QnA.Common.Persistence.HangfireQnaDb.Configuration;
using Querify.QnA.Common.Persistence.HangfireQnaDb.DbContext;

namespace Querify.QnA.Common.Persistence.HangfireQnaDb.Factories;

public sealed class HangfireQnaDbContextFactory : IDesignTimeDbContextFactory<HangfireQnaDbContext>
{
    public HangfireQnaDbContext CreateDbContext(string[] args)
    {
        var solutionRoot = FindSolutionRoot(Directory.GetCurrentDirectory());
        var environment = ResolveEnvironment();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(solutionRoot, "dotnet", "Querify.QnA.Worker.Api"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = HangfireQnaDbConfiguration.GetConnectionString(configuration);
        var migrationsAssembly = typeof(HangfireQnaDbContext).Assembly.GetName().Name;
        var options = new DbContextOptionsBuilder<HangfireQnaDbContext>()
            .UseNpgsql(
                connectionString,
                builder => builder.MigrationsAssembly(migrationsAssembly))
            .Options;

        return new HangfireQnaDbContext(options);
    }

    private static string ResolveEnvironment()
    {
        return Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
               ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
               ?? "Development";
    }

    private static string FindSolutionRoot(string startDirectory)
    {
        var directory = new DirectoryInfo(startDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Querify.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate Querify.sln from the current directory.");
    }
}
