namespace Querify.Tools.Migration.Utilities;

internal static class SolutionRootLocator
{
    public static string? Find()
    {
        var current = Directory.GetCurrentDirectory();
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "Querify.sln")))
            {
                return current;
            }

            var parent = Directory.GetParent(current);
            if (parent is null)
            {
                return null;
            }

            current = parent.FullName;
        }

        return null;
    }
}