using BaseFaq.Models.Common.Enums;

namespace BaseFaq.Tools.Migration.Prompts;

internal static class MigrationPrompt
{
    public static AppEnum SelectApp()
    {
        Console.WriteLine("Which AppEnum?");
        Console.WriteLine($"1) {AppEnum.Faq}");
        Console.WriteLine($"2) {AppEnum.QnA}");

        while (true)
        {
            Console.Write($"Select (default {AppEnum.Faq}): ");
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                return AppEnum.Faq;
            }

            if (string.Equals(input, "1", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, AppEnum.Faq.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return AppEnum.Faq;
            }

            if (string.Equals(input, "2", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, AppEnum.QnA.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return AppEnum.QnA;
            }

            Console.WriteLine("Invalid AppEnum value.");
        }
    }

    public static MigrationCommand SelectCommand()
    {
        Console.WriteLine("Which command?");
        Console.WriteLine("1) Migrations add");
        Console.WriteLine("2) Database update");

        while (true)
        {
            Console.Write("Select (1 or 2): ");
            var input = Console.ReadLine();
            if (string.Equals(input, "1", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "migrations add", StringComparison.OrdinalIgnoreCase))
            {
                return MigrationCommand.MigrationsAdd;
            }

            if (string.Equals(input, "2", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "database update", StringComparison.OrdinalIgnoreCase))
            {
                return MigrationCommand.DatabaseUpdate;
            }

            Console.WriteLine("Invalid command.");
        }
    }

    public static string ReadMigrationName()
    {
        while (true)
        {
            Console.Write("Migration name: ");
            var input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input))
            {
                return input.Trim();
            }

            Console.WriteLine("Migration name is required.");
        }
    }
}
