using Querify.Models.Common.Enums;
using Querify.Tools.Migration.Prompts;

namespace Querify.Tools.Migration.Configuration;

internal sealed class MigrationCliArguments
{
    public ModuleEnum? Module { get; private set; }
    public MigrationCommand? Command { get; private set; }
    public string? MigrationName { get; private set; }

    public static MigrationCliArguments Parse(string[] args)
    {
        var parsed = new MigrationCliArguments();

        for (var i = 0; i < args.Length; i++)
        {
            var argument = args[i];
            if (!argument.StartsWith("--", StringComparison.Ordinal))
            {
                continue;
            }

            switch (argument.ToLowerInvariant())
            {
                case "--module":
                    EnsureValue(args, i, argument);
                    parsed.Module = ParseModule(args[++i]);
                    break;
                case "--command":
                    EnsureValue(args, i, argument);
                    parsed.Command = ParseCommand(args[++i]);
                    break;
                case "--migration-name":
                    EnsureValue(args, i, argument);
                    parsed.MigrationName = ParseMigrationName(args[++i]);
                    break;
                default:
                    if (i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal))
                    {
                        i++;
                    }

                    break;
            }
        }

        return parsed;
    }

    private static void EnsureValue(string[] args, int index, string argument)
    {
        if (index + 1 >= args.Length || args[index + 1].StartsWith("--", StringComparison.Ordinal))
        {
            throw new ArgumentException($"Missing value for argument '{argument}'.");
        }
    }

    private static ModuleEnum ParseModule(string value)
    {
        if (string.Equals(value, "1", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, ModuleEnum.QnA.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return ModuleEnum.QnA;
        }

        throw new ArgumentException($"Unsupported module '{value}'. Supported: {ModuleEnum.QnA}.");
    }

    private static MigrationCommand ParseCommand(string value)
    {
        if (string.Equals(value, "1", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "migrations-add", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "migrations add", StringComparison.OrdinalIgnoreCase))
        {
            return MigrationCommand.MigrationsAdd;
        }

        if (string.Equals(value, "2", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "database-update", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "database update", StringComparison.OrdinalIgnoreCase))
        {
            return MigrationCommand.DatabaseUpdate;
        }

        throw new ArgumentException(
            $"Unsupported command '{value}'. Supported: migrations-add, database-update.");
    }

    private static string ParseMigrationName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Migration name must not be empty.");
        }

        return value.Trim();
    }
}
