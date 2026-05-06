using System.Diagnostics;
using Querify.Models.Common.Enums;

namespace Querify.Tools.Migration.Runners;

internal static class EfMigrationsRunner
{
    public static int AddMigration(string solutionRoot, string migrationName, ModuleEnum module)
    {
        var projectPath = ResolveProjectPath(solutionRoot, module);

        var startupProjectPath = Path.Combine(
            solutionRoot,
            "dotnet",
            "Querify.Tools.Migration",
            "Querify.Tools.Migration.csproj");

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
        processInfo.ArgumentList.Add(ResolveContextName(module));
        processInfo.ArgumentList.Add("--project");
        processInfo.ArgumentList.Add(projectPath);
        processInfo.ArgumentList.Add("--startup-project");
        processInfo.ArgumentList.Add(startupProjectPath);
        processInfo.ArgumentList.Add("--");
        processInfo.ArgumentList.Add("--module");
        processInfo.ArgumentList.Add(module.ToString());

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

    private static string ResolveProjectPath(string solutionRoot, ModuleEnum module)
    {
        if (module != ModuleEnum.QnA)
        {
            throw new InvalidOperationException($"Migrations are not supported for {module}.");
        }

        return Path.Combine(
            solutionRoot,
            "dotnet",
            "Querify.QnA.Common.Persistence.QnADb",
            "Querify.QnA.Common.Persistence.QnADb.csproj");
    }

    private static string ResolveContextName(ModuleEnum module)
    {
        return module == ModuleEnum.QnA
            ? "QnADbContext"
            : throw new InvalidOperationException($"Migrations are not supported for {module}.");
    }
}
