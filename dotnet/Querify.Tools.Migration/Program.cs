using Querify.Tools.Migration.Configuration;
using Querify.Tools.Migration.Prompts;
using Querify.Tools.Migration.Runners;
using Querify.Tools.Migration.Utilities;
using Querify.Models.Common.Enums;

namespace Querify.Tools.Migration;

public static class Program
{
    public static int Main(string[] args)
    {
        if (IsEfDesignTimeInvocation(args))
        {
            return 0;
        }

        MigrationCliArguments cliArguments;
        try
        {
            cliArguments = MigrationCliArguments.Parse(args);
        }
        catch (ArgumentException ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }

        var cliMode = args.Length > 0;

        var module = cliArguments.Module ?? ModuleEnum.QnA;
        MigrationCommand command;

        if (cliMode)
        {
            if (cliArguments.Command is null)
            {
                Console.Error.WriteLine("Missing --command in CLI mode.");
                return 1;
            }

            command = cliArguments.Command.Value;
        }
        else
        {
            command = MigrationPrompt.SelectCommand();
        }

        var solutionRoot = SolutionRootLocator.Find();
        var configuration = MigrationsConfiguration.Build(solutionRoot);

        string tenantDbConnectionString;
        try
        {
            tenantDbConnectionString = MigrationsConfiguration.GetTenantDbConnectionString(configuration);
        }
        catch (InvalidOperationException ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }

        if (command == MigrationCommand.DatabaseUpdate)
        {
            TenantMigrationUpdater.ApplyAll(configuration, tenantDbConnectionString, module);
            return 0;
        }

        if (solutionRoot is null)
        {
            Console.Error.WriteLine("Unable to locate solution root (Querify.sln).");
            return 1;
        }

        var migrationName = cliMode
            ? cliArguments.MigrationName
            : MigrationPrompt.ReadMigrationName();

        if (string.IsNullOrWhiteSpace(migrationName))
        {
            Console.Error.WriteLine("Migration name is required for migrations-add command.");
            return 1;
        }

        return EfMigrationsRunner.AddMigration(solutionRoot, migrationName, module);
    }

    private static bool IsEfDesignTimeInvocation(string[] args)
    {
        return args.Any(arg =>
            string.Equals(arg, "--applicationName", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(arg, "--projectDir", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(arg, "--rootnamespace", StringComparison.OrdinalIgnoreCase));
    }
}
