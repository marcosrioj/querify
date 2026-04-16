using System.Diagnostics;
using BaseFaq.Models.Common.Enums;

namespace BaseFaq.Tools.Migration.Runners;

internal static class EfMigrationsRunner
{
    public static int AddMigration(string solutionRoot, string migrationName, AppEnum app)
    {
        var projectPath = ResolveProjectPath(solutionRoot, app);

        var startupProjectPath = Path.Combine(
            solutionRoot,
            "dotnet",
            "BaseFaq.Tools.Migration",
            "BaseFaq.Tools.Migration.csproj");

        if (!File.Exists(projectPath))
        {
            Console.Error.WriteLine($"Migration project not found: {projectPath}");
            return 1;
        }

        if (!File.Exists(startupProjectPath))
        {
            Console.Error.WriteLine($"Startup project not found: {startupProjectPath}");
            return 1;
        }

        var processInfo = new ProcessStartInfo("dotnet")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = solutionRoot
        };

        processInfo.ArgumentList.Add("ef");
        processInfo.ArgumentList.Add("migrations");
        processInfo.ArgumentList.Add("add");
        processInfo.ArgumentList.Add(migrationName);
        processInfo.ArgumentList.Add("--context");
        processInfo.ArgumentList.Add(ResolveContextName(app));
        processInfo.ArgumentList.Add("--project");
        processInfo.ArgumentList.Add(projectPath);
        processInfo.ArgumentList.Add("--startup-project");
        processInfo.ArgumentList.Add(startupProjectPath);
        processInfo.ArgumentList.Add("--");
        processInfo.ArgumentList.Add("--app");
        processInfo.ArgumentList.Add(app.ToString());

        using var process = Process.Start(processInfo);
        if (process is null)
        {
            Console.Error.WriteLine("Failed to start dotnet ef process.");
            return 1;
        }

        process.OutputDataReceived += (_, eventArgs) =>
        {
            if (!string.IsNullOrWhiteSpace(eventArgs.Data))
            {
                Console.WriteLine(eventArgs.Data);
            }
        };
        process.ErrorDataReceived += (_, eventArgs) =>
        {
            if (!string.IsNullOrWhiteSpace(eventArgs.Data))
            {
                Console.Error.WriteLine(eventArgs.Data);
            }
        };

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();

        return process.ExitCode;
    }

    private static string ResolveProjectPath(string solutionRoot, AppEnum app)
    {
        return app switch
        {
            AppEnum.Faq => Path.Combine(
                solutionRoot,
                "dotnet",
                "BaseFaq.Faq.Common.Persistence.FaqDb",
                "BaseFaq.Faq.Common.Persistence.FaqDb.csproj"),
            AppEnum.QnA => Path.Combine(
                solutionRoot,
                "dotnet",
                "BaseFaq.QnA.Common.Persistence.QnADb",
                "BaseFaq.QnA.Common.Persistence.QnADb.csproj"),
            _ => throw new InvalidOperationException($"Migrations are not supported for {app}.")
        };
    }

    private static string ResolveContextName(AppEnum app)
    {
        return app switch
        {
            AppEnum.Faq => "FaqDbContext",
            AppEnum.QnA => "QnADbContext",
            _ => throw new InvalidOperationException($"Migrations are not supported for {app}.")
        };
    }
}
