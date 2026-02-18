using System.Text.RegularExpressions;
using Xunit;

namespace BaseFaq.Common.Architecture.Test.IntegrationTest;

public class ProjectRulesComplianceTests
{
    private static readonly HashSet<string> AllowedSimpleTypes = new(StringComparer.Ordinal)
    {
        "Guid",
        "System.Guid",
        "bool",
        "System.Boolean",
        "string",
        "System.String"
    };

    private static readonly string[] ScopedProjectPrefixes =
    [
        "BaseFaq.Faq.",
        "BaseFaq.Tenant.",
        "BaseFaq.Common.",
        "BaseFaq.Models.Common",
        "BaseFaq.Models.Faq",
        "BaseFaq.Models.Tenant",
        "BaseFaq.Models.User",
        "BaseFaq.Tools.",
        "BaseFaq.AI."
    ];

    private static readonly Regex CommandDeclarationRegex =
        new(@"\b(?:class|record)\s+\w+(?:\s*\([^\)]*\))?\s*:\s*IRequest(?:<\s*(?<type>[^>]+)\s*>)?",
            RegexOptions.Multiline | RegexOptions.Compiled);

    private static readonly Regex ControllerMethodRegex =
        new(@"(?<attrs>(?:\s*\[[^\]]+\]\s*)+)\s*public\s+(?:async\s+)?Task<IActionResult>\s+\w+\s*\(",
            RegexOptions.Multiline | RegexOptions.Compiled);

    private static readonly Regex ProducesResponseTypeRegex =
        new(@"ProducesResponseType\s*\(", RegexOptions.Compiled);

    private static readonly Regex ProducesResponseTypeWithTypeRegex =
        new(@"ProducesResponseType\s*\(\s*typeof\s*\(\s*(?<type>[^\)]+)\s*\)",
            RegexOptions.Compiled);

    private static readonly Regex CommandResponseWrapperRegex =
        new(@"\b(?:class|record)\s+\w*(AcceptedResponse|CommandResponse|WriteResponse|ResponseWrapper)\w*",
            RegexOptions.Compiled);

    private static readonly string RepositoryRoot = FindRepositoryRoot();
    private static readonly string DotnetRoot = Path.Combine(RepositoryRoot, "dotnet");

    [Fact]
    public void Commands_MustReturnSimpleTypesOnly()
    {
        var failures = new List<string>();

        foreach (var filePath in EnumerateSourceFiles())
        {
            if (!filePath.Contains($"{Path.DirectorySeparatorChar}Commands{Path.DirectorySeparatorChar}",
                    StringComparison.Ordinal))
            {
                continue;
            }

            if (!Path.GetFileName(filePath).EndsWith("Command.cs", StringComparison.Ordinal))
            {
                continue;
            }

            var source = File.ReadAllText(filePath);
            var matches = CommandDeclarationRegex.Matches(source);
            if (matches.Count == 0)
            {
                failures.Add($"{ToRelativePath(filePath)}: command declaration implementing IRequest was not found.");
                continue;
            }

            foreach (Match match in matches)
            {
                var responseType = match.Groups["type"];
                if (!responseType.Success)
                {
                    continue;
                }

                var normalizedType = NormalizeType(responseType.Value);
                if (AllowedSimpleTypes.Contains(normalizedType))
                {
                    continue;
                }

                failures.Add(
                    $"{ToRelativePath(filePath)}: command response type '{responseType.Value.Trim()}' is not allowed.");
            }
        }

        Assert.True(failures.Count == 0, BuildFailureMessage(failures));
    }

    [Fact]
    public void WriteEndpoints_MustDeclareSimpleResponseTypes()
    {
        var failures = new List<string>();

        foreach (var filePath in EnumerateSourceFiles())
        {
            if (!filePath.Contains($"{Path.DirectorySeparatorChar}Controllers{Path.DirectorySeparatorChar}",
                    StringComparison.Ordinal))
            {
                continue;
            }

            var source = File.ReadAllText(filePath);
            var methodMatches = ControllerMethodRegex.Matches(source);

            foreach (Match methodMatch in methodMatches)
            {
                var attributes = methodMatch.Groups["attrs"].Value;
                if (!IsWriteEndpoint(attributes))
                {
                    continue;
                }

                var responseMatches = ProducesResponseTypeRegex.Matches(attributes);
                if (responseMatches.Count == 0)
                {
                    failures.Add(
                        $"{ToRelativePath(filePath)}: write endpoint is missing [ProducesResponseType] declaration.");
                    continue;
                }

                var typedResponseMatches = ProducesResponseTypeWithTypeRegex.Matches(attributes);
                if (typedResponseMatches.Count == 0)
                {
                    failures.Add(
                        $"{ToRelativePath(filePath)}: write endpoint response must declare typeof(Guid|bool|string).");
                    continue;
                }

                foreach (Match responseMatch in typedResponseMatches)
                {
                    var normalizedType = NormalizeType(responseMatch.Groups["type"].Value);
                    if (AllowedSimpleTypes.Contains(normalizedType))
                    {
                        continue;
                    }

                    failures.Add(
                        $"{ToRelativePath(filePath)}: write endpoint response type '{responseMatch.Groups["type"].Value.Trim()}' is not allowed.");
                }
            }
        }

        Assert.True(failures.Count == 0, BuildFailureMessage(failures));
    }

    [Fact]
    public void CommandResponseWrappers_MustNotBeIntroduced()
    {
        var failures = new List<string>();

        foreach (var filePath in EnumerateSourceFiles())
        {
            var source = File.ReadAllText(filePath);
            if (!CommandResponseWrapperRegex.IsMatch(source))
            {
                continue;
            }

            failures.Add($"{ToRelativePath(filePath)}: command response wrapper pattern found.");
        }

        Assert.True(failures.Count == 0, BuildFailureMessage(failures));
    }

    private static IEnumerable<string> EnumerateSourceFiles()
    {
        return Directory.EnumerateFiles(DotnetRoot, "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}",
                StringComparison.Ordinal))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}",
                StringComparison.Ordinal))
            .Where(IsInScopedProject);
    }

    private static bool IsInScopedProject(string filePath)
    {
        var relativePath = Path.GetRelativePath(DotnetRoot, filePath);
        var firstSeparatorIndex = relativePath.IndexOf(Path.DirectorySeparatorChar);
        if (firstSeparatorIndex <= 0)
        {
            return false;
        }

        var projectName = relativePath[..firstSeparatorIndex];
        return ScopedProjectPrefixes.Any(projectName.StartsWith);
    }

    private static bool IsWriteEndpoint(string attributes)
    {
        return attributes.Contains("[HttpPost", StringComparison.Ordinal) ||
               attributes.Contains("[HttpPut", StringComparison.Ordinal) ||
               attributes.Contains("[HttpPatch", StringComparison.Ordinal);
    }

    private static string NormalizeType(string typeName)
    {
        return Regex.Replace(typeName, @"\s+", string.Empty).Replace("?", string.Empty);
    }

    private static string BuildFailureMessage(IEnumerable<string> failures)
    {
        var failureList = failures.ToList();
        if (failureList.Count == 0)
        {
            return string.Empty;
        }

        return "PROJECT_RULES.md compliance failures:" + Environment.NewLine +
               string.Join(Environment.NewLine, failureList);
    }

    private static string ToRelativePath(string fullPath)
    {
        return Path.GetRelativePath(RepositoryRoot, fullPath).Replace('\\', '/');
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var projectRulesPath = Path.Combine(current.FullName, "PROJECT_RULES.md");
            if (File.Exists(projectRulesPath))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException("Could not locate repository root containing PROJECT_RULES.md.");
    }
}