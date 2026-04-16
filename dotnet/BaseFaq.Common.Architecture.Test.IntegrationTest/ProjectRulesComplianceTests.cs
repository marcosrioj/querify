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
        "BaseFaq.QnA.",
        "BaseFaq.Tenant.",
        "BaseFaq.Common.",
        "BaseFaq.Models.Common",
        "BaseFaq.Models.Faq",
        "BaseFaq.Models.QnA",
        "BaseFaq.Models.Tenant",
        "BaseFaq.Models.User",
        "BaseFaq.Tools.",
        "BaseFaq.AI."
    ];

    private static readonly string[] ProhibitedQnAMonolithProjects =
    [
        "dotnet/BaseFaq.QnA.Portal.Business/BaseFaq.QnA.Portal.Business.csproj",
        "dotnet/BaseFaq.QnA.Public.Business/BaseFaq.QnA.Public.Business.csproj"
    ];

    private static readonly string[] RequiredQnAFeatureProjects =
    [
        "dotnet/BaseFaq.QnA.Portal.Business.Answer/BaseFaq.QnA.Portal.Business.Answer.csproj",
        "dotnet/BaseFaq.QnA.Portal.Business.KnowledgeSource/BaseFaq.QnA.Portal.Business.KnowledgeSource.csproj",
        "dotnet/BaseFaq.QnA.Portal.Business.Question/BaseFaq.QnA.Portal.Business.Question.csproj",
        "dotnet/BaseFaq.QnA.Portal.Business.QuestionSpace/BaseFaq.QnA.Portal.Business.QuestionSpace.csproj",
        "dotnet/BaseFaq.QnA.Portal.Business.ThreadActivity/BaseFaq.QnA.Portal.Business.ThreadActivity.csproj",
        "dotnet/BaseFaq.QnA.Portal.Business.Topic/BaseFaq.QnA.Portal.Business.Topic.csproj",
        "dotnet/BaseFaq.QnA.Public.Business.Feedback/BaseFaq.QnA.Public.Business.Feedback.csproj",
        "dotnet/BaseFaq.QnA.Public.Business.Question/BaseFaq.QnA.Public.Business.Question.csproj",
        "dotnet/BaseFaq.QnA.Public.Business.QuestionSpace/BaseFaq.QnA.Public.Business.QuestionSpace.csproj",
        "dotnet/BaseFaq.QnA.Public.Business.Vote/BaseFaq.QnA.Public.Business.Vote.csproj"
    ];

    private static readonly string[] RequiredQnAModelDtoDirectories =
    [
        "dotnet/BaseFaq.Models.QnA/Dtos/Answer",
        "dotnet/BaseFaq.Models.QnA/Dtos/KnowledgeSource",
        "dotnet/BaseFaq.Models.QnA/Dtos/Question",
        "dotnet/BaseFaq.Models.QnA/Dtos/QuestionSpace",
        "dotnet/BaseFaq.Models.QnA/Dtos/ThreadActivity",
        "dotnet/BaseFaq.Models.QnA/Dtos/Topic"
    ];

    private static readonly string[] ProhibitedQnAModelDtoDirectories =
    [
        "dotnet/BaseFaq.Models.QnA/Dtos/Link"
    ];

    private static readonly string[] RequiredQnAModelDtoFiles =
    [
        "dotnet/BaseFaq.Models.QnA/Dtos/Answer/AnswerDto.cs",
        "dotnet/BaseFaq.Models.QnA/Dtos/Answer/AnswerSourceLinkDto.cs",
        "dotnet/BaseFaq.Models.QnA/Dtos/KnowledgeSource/KnowledgeSourceDto.cs",
        "dotnet/BaseFaq.Models.QnA/Dtos/Question/QuestionDto.cs",
        "dotnet/BaseFaq.Models.QnA/Dtos/Question/QuestionSourceLinkDto.cs",
        "dotnet/BaseFaq.Models.QnA/Dtos/Question/QuestionTopicDto.cs",
        "dotnet/BaseFaq.Models.QnA/Dtos/QuestionSpace/QuestionSpaceDto.cs",
        "dotnet/BaseFaq.Models.QnA/Dtos/QuestionSpace/QuestionSpaceSourceDto.cs",
        "dotnet/BaseFaq.Models.QnA/Dtos/QuestionSpace/QuestionSpaceTopicDto.cs",
        "dotnet/BaseFaq.Models.QnA/Dtos/ThreadActivity/ThreadActivityDto.cs",
        "dotnet/BaseFaq.Models.QnA/Dtos/Topic/TopicDto.cs"
    ];

    private static readonly HashSet<string> AllowedQnAHelperFiles = new(StringComparer.Ordinal)
    {
        "dotnet/BaseFaq.QnA.Public.Business.Feedback/Helpers/FeedbackRequestContext.cs",
        "dotnet/BaseFaq.QnA.Public.Business.Vote/Helpers/VoteRequestContext.cs"
    };

    private static readonly Regex CommandDeclarationRegex =
        new(@"\b(?:class|record)\s+\w+(?:\s*\([^\)]*\))?\s*:\s*IRequest(?:<\s*(?<type>[^>]+)\s*>)?",
            RegexOptions.Multiline | RegexOptions.Compiled);

    private static readonly Regex CommandHandlerResponseTypeRegex =
        new(@"IRequestHandler\s*<\s*[^,>]+Command\s*,\s*(?<type>[^>]+)\s*>",
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

    private static readonly Regex QnAEntityMethodRegex =
        new(@"^\s*(?:public|internal|protected|private)\s+(?:static\s+)?(?:async\s+)?[A-Za-z_][A-Za-z0-9_<>,\.\?\[\]]*\s+[A-Za-z_][A-Za-z0-9_]*\s*\(",
            RegexOptions.Multiline | RegexOptions.Compiled);

    private static readonly Regex QnAEntityConstructorRegex =
        new(@"^\s*(?:public|internal|protected|private)\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*\(",
            RegexOptions.Multiline | RegexOptions.Compiled);

    private static readonly Regex QnARequestDtoInheritanceRegex =
        new(@"\bclass\s+(?<name>\w*RequestDto)\s*:\s*(?<base>[A-Za-z_][A-Za-z0-9_<>,\.\?]*)",
            RegexOptions.Multiline | RegexOptions.Compiled);

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
    public void CommandHandlers_MustReturnSimpleTypesOnly()
    {
        var failures = new List<string>();

        foreach (var filePath in EnumerateSourceFiles())
        {
            if (!filePath.Contains($"{Path.DirectorySeparatorChar}Commands{Path.DirectorySeparatorChar}",
                    StringComparison.Ordinal))
            {
                continue;
            }

            if (!Path.GetFileName(filePath).EndsWith("CommandHandler.cs", StringComparison.Ordinal))
            {
                continue;
            }

            var source = File.ReadAllText(filePath);
            foreach (Match match in CommandHandlerResponseTypeRegex.Matches(source))
            {
                var normalizedType = NormalizeType(match.Groups["type"].Value);
                if (AllowedSimpleTypes.Contains(normalizedType))
                {
                    continue;
                }

                failures.Add(
                    $"{ToRelativePath(filePath)}: command handler response type '{match.Groups["type"].Value.Trim()}' is not allowed.");
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

    [Fact]
    public void QnABusinessProjects_MustMirrorFaqFeatureBoundaries()
    {
        var failures = new List<string>();

        foreach (var relativePath in ProhibitedQnAMonolithProjects)
        {
            if (!File.Exists(ToAbsolutePath(relativePath)))
            {
                continue;
            }

            failures.Add($"{relativePath}: monolithic QnA business project is not allowed.");
        }

        foreach (var relativePath in RequiredQnAFeatureProjects)
        {
            if (File.Exists(ToAbsolutePath(relativePath)))
            {
                continue;
            }

            failures.Add($"{relativePath}: required QnA feature project is missing.");
        }

        Assert.True(failures.Count == 0, BuildFailureMessage(failures));
    }

    [Fact]
    public void QnABusinessProjects_MustUseRealFilesInsideOwningProject()
    {
        var failures = new List<string>();

        foreach (var relativePath in RequiredQnAFeatureProjects)
        {
            var projectPath = ToAbsolutePath(relativePath);
            if (!File.Exists(projectPath))
            {
                continue;
            }

            var projectContent = File.ReadAllText(projectPath);
            if (projectContent.Contains("<Compile Include=\"..\\", StringComparison.OrdinalIgnoreCase) ||
                projectContent.Contains("<Compile Include=\"../", StringComparison.OrdinalIgnoreCase) ||
                projectContent.Contains("<Link>", StringComparison.OrdinalIgnoreCase))
            {
                failures.Add($"{relativePath}: linked source files are not allowed; keep real files inside the owning project directory.");
            }

            var projectDirectory = Path.GetDirectoryName(projectPath)!;
            var sourceFiles = Directory.EnumerateFiles(projectDirectory, "*.cs", SearchOption.AllDirectories)
                .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
                .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
                .ToList();

            if (sourceFiles.Count == 0)
            {
                failures.Add($"{relativePath}: feature project has no real source files inside its directory.");
            }
        }

        Assert.True(failures.Count == 0, BuildFailureMessage(failures));
    }

    [Fact]
    public void QnAModels_MustMirrorFaqDtoFoldersAndFiles()
    {
        var failures = new List<string>();

        foreach (var relativePath in RequiredQnAModelDtoDirectories)
        {
            if (Directory.Exists(ToAbsolutePath(relativePath)))
            {
                continue;
            }

            failures.Add($"{relativePath}: required QnA DTO directory is missing.");
        }

        foreach (var relativePath in RequiredQnAModelDtoFiles)
        {
            if (File.Exists(ToAbsolutePath(relativePath)))
            {
                continue;
            }

            failures.Add($"{relativePath}: required QnA DTO file is missing.");
        }

        foreach (var relativePath in ProhibitedQnAModelDtoDirectories)
        {
            if (!Directory.Exists(ToAbsolutePath(relativePath)))
            {
                continue;
            }

            failures.Add($"{relativePath}: pseudo-entity DTO directories are not allowed in BaseFaq.Models.QnA.");
        }

        var qnaDtosRoot = ToAbsolutePath("dotnet/BaseFaq.Models.QnA/Dtos");
        if (Directory.Exists(qnaDtosRoot))
        {
            foreach (var filePath in Directory.EnumerateFiles(qnaDtosRoot, "*.cs", SearchOption.TopDirectoryOnly))
            {
                failures.Add($"{ToRelativePath(filePath)}: DTO files must live in FAQ-style feature folders, not directly under Dtos.");
            }

            foreach (var filePath in Directory.EnumerateFiles(qnaDtosRoot, "*Dtos.cs", SearchOption.AllDirectories))
            {
                failures.Add($"{ToRelativePath(filePath)}: aggregate *Dtos.cs files are not allowed in BaseFaq.Models.QnA.");
            }
        }

        Assert.True(failures.Count == 0, BuildFailureMessage(failures));
    }

    [Fact]
    public void QnARequestDtos_MustUseAllowedInheritanceOnly()
    {
        var failures = new List<string>();
        var qnaDtosRoot = ToAbsolutePath("dotnet/BaseFaq.Models.QnA/Dtos");

        foreach (var filePath in Directory.EnumerateFiles(qnaDtosRoot, "*RequestDto.cs", SearchOption.AllDirectories))
        {
            var source = File.ReadAllText(filePath);
            var match = QnARequestDtoInheritanceRegex.Match(source);
            if (!match.Success)
            {
                continue;
            }

            var dtoName = match.Groups["name"].Value;
            var baseType = NormalizeType(match.Groups["base"].Value);
            var isAllowedPagedQueryDto =
                dtoName.EndsWith("GetAllRequestDto", StringComparison.Ordinal) &&
                string.Equals(baseType, "PagedAndSortedResultRequestDto", StringComparison.Ordinal);

            if (isAllowedPagedQueryDto)
            {
                continue;
            }

            failures.Add($"{ToRelativePath(filePath)}: request DTO inheritance from '{match.Groups["base"].Value}' is not allowed for '{dtoName}' in BaseFaq.Models.QnA.");
        }

        Assert.True(failures.Count == 0, BuildFailureMessage(failures));
    }

    [Fact]
    public void QnABusinessProjects_MustNotKeepGenericHelperFiles()
    {
        var failures = new List<string>();

        foreach (var relativePath in RequiredQnAFeatureProjects)
        {
            var projectPath = ToAbsolutePath(relativePath);
            if (!File.Exists(projectPath))
            {
                continue;
            }

            var projectDirectory = Path.GetDirectoryName(projectPath)!;
            var helpersDirectory = Path.Combine(projectDirectory, "Helpers");
            if (!Directory.Exists(helpersDirectory))
            {
                continue;
            }

            foreach (var helperPath in Directory.EnumerateFiles(helpersDirectory, "*.cs", SearchOption.AllDirectories))
            {
                var relativeHelperPath = ToRelativePath(helperPath);
                if (AllowedQnAHelperFiles.Contains(relativeHelperPath))
                {
                    continue;
                }

                failures.Add($"{relativeHelperPath}: generic QnA helper files are not allowed; move logic into commands/queries or use FAQ-style request-context helpers only.");
            }
        }

        Assert.True(failures.Count == 0, BuildFailureMessage(failures));
    }

    [Fact]
    public void QnAPersistenceEntities_MustStayAnemic()
    {
        var failures = new List<string>();
        var entitiesDirectory = ToAbsolutePath("dotnet/BaseFaq.QnA.Common.Persistence.QnADb/Entities");

        foreach (var entityPath in Directory.EnumerateFiles(entitiesDirectory, "*.cs", SearchOption.TopDirectoryOnly))
        {
            var source = File.ReadAllText(entityPath);
            var relativePath = ToRelativePath(entityPath);

            if (source.Contains("[NotMapped]", StringComparison.Ordinal))
            {
                failures.Add($"{relativePath}: [NotMapped] convenience projections are not allowed in QnA persistence entities.");
            }

            foreach (Match match in QnAEntityMethodRegex.Matches(source))
            {
                failures.Add($"{relativePath}: behavior method '{match.Value.Trim()}' is not allowed in QnA persistence entities.");
            }

            foreach (Match match in QnAEntityConstructorRegex.Matches(source))
            {
                var typeName = Path.GetFileNameWithoutExtension(entityPath);
                var constructorName = match.Groups["name"].Value;
                if (!string.Equals(constructorName, typeName, StringComparison.Ordinal))
                {
                    continue;
                }

                failures.Add($"{relativePath}: constructor '{constructorName}(...)' is not allowed in QnA persistence entities.");
            }
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

    private static string ToAbsolutePath(string relativePath)
    {
        return Path.Combine(RepositoryRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
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
