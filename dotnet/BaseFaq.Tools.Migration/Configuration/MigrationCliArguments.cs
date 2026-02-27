using BaseFaq.Models.Common.Enums;
using BaseFaq.Tools.Migration.Prompts;

namespace BaseFaq.Tools.Migration.Configuration;

internal sealed class MigrationCliArguments
{
    public AppEnum? App { get; private init; }
    public MigrationCommand? Command { get; private init; }
    public string? MigrationName { get; private init; }

    public static MigrationCliArguments Parse(string[] args)
    {
        var parsed = new MigrationCliArguments();

        for (var i = 0; i < args.Length; i++)
        {
            var argument = args[i];
            if (!argument.StartsWith("--", StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    $"Invalid argument '{argument}'. Use --app, --command, --migration-name.");
            }

            if (i + 1 >= args.Length)
            {
                throw new ArgumentException($"Missing value for argument '{argument}'.");
            }

            var value = args[++i];

            switch (argument.ToLowerInvariant())
            {
                case "--app":
                    parsed.App = ParseApp(value);
                    break;
                case "--command":
                    parsed.Command = ParseCommand(value);
                    break;
                case "--migration-name":
                    parsed.MigrationName = ParseMigrationName(value);
                    break;
                default:
                    throw new ArgumentException(
                        $"Unknown argument '{argument}'. Supported: --app, --command, --migration-name.");
            }
        }

        return parsed;
    }

    private static AppEnum ParseApp(string value)
    {
        if (string.Equals(value, "1", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, AppEnum.Faq.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return AppEnum.Faq;
        }

        throw new ArgumentException($"Unsupported app '{value}'. Supported: {AppEnum.Faq}.");
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