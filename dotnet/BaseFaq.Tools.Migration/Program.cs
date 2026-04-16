using BaseFaq.Tools.Migration.Configuration;
using BaseFaq.Tools.Migration.Prompts;
using BaseFaq.Tools.Migration.Runners;
using BaseFaq.Tools.Migration.Utilities;
using BaseFaq.Models.Common.Enums;

namespace BaseFaq.Tools.Migration;

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

        AppEnum app;
        MigrationCommand command;

        if (cliMode)
        {
            app = cliArguments.App ?? AppEnum.Faq;

            if (cliArguments.Command is null)
            {
                Console.Error.WriteLine("Missing --command in CLI mode.");
                return 1;
            }

            command = cliArguments.Command.Value;
        }
        else
        {
            app = MigrationPrompt.SelectApp();
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
            FaqTenantMigrationUpdater.ApplyAll(configuration, tenantDbConnectionString, app);
            return 0;
        }

        if (solutionRoot is null)
        {
            Console.Error.WriteLine("Unable to locate solution root (BaseFaq.sln).");
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

        return EfMigrationsRunner.AddMigration(solutionRoot, migrationName, app);
    }

    private static bool IsEfDesignTimeInvocation(string[] args)
    {
        return args.Any(arg =>
            string.Equals(arg, "--applicationName", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(arg, "--projectDir", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(arg, "--rootnamespace", StringComparison.OrdinalIgnoreCase));
    }
}
