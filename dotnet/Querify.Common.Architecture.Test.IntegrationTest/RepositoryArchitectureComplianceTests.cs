using System.Text.RegularExpressions;
using Xunit;

namespace Querify.Common.Architecture.Test.IntegrationTest;

public class RepositoryArchitectureComplianceTests
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

    private static readonly HashSet<string> AllowedCommandResponseExceptions = new(StringComparer.Ordinal)
    {
        "dotnet/Querify.QnA.Portal.Business.Source/Commands/CreateUploadIntent/SourcesCreateUploadIntentCommand.cs|SourceUploadIntentResponseDto",
        "dotnet/Querify.QnA.Portal.Business.Source/Commands/CreateUploadIntent/SourcesCreateUploadIntentCommandHandler.cs|SourceUploadIntentResponseDto"
    };

    private static readonly string[] ScopedProjectPrefixes =
    [
        "Querify.QnA.",
        "Querify.Tenant.",
        "Querify.Common.",
        "Querify.Models.Common",
        "Querify.Models.QnA",
        "Querify.Models.Tenant",
        "Querify.Models.User",
        "Querify.Tools."
    ];

    private static readonly string[] ProhibitedQnAMonolithProjects =
    [
        "dotnet/Querify.QnA.Portal.Business/Querify.QnA.Portal.Business.csproj",
        "dotnet/Querify.QnA.Public.Business/Querify.QnA.Public.Business.csproj"
    ];

    private static readonly string[] RequiredQnAFeatureProjects =
    [
        "dotnet/Querify.QnA.Portal.Business.Answer/Querify.QnA.Portal.Business.Answer.csproj",
        "dotnet/Querify.QnA.Portal.Business.Source/Querify.QnA.Portal.Business.Source.csproj",
        "dotnet/Querify.QnA.Portal.Business.Question/Querify.QnA.Portal.Business.Question.csproj",
        "dotnet/Querify.QnA.Portal.Business.Space/Querify.QnA.Portal.Business.Space.csproj",
        "dotnet/Querify.QnA.Portal.Business.Activity/Querify.QnA.Portal.Business.Activity.csproj",
        "dotnet/Querify.QnA.Portal.Business.Tag/Querify.QnA.Portal.Business.Tag.csproj",
        "dotnet/Querify.QnA.Public.Business.Feedback/Querify.QnA.Public.Business.Feedback.csproj",
        "dotnet/Querify.QnA.Public.Business.Question/Querify.QnA.Public.Business.Question.csproj",
        "dotnet/Querify.QnA.Public.Business.Space/Querify.QnA.Public.Business.Space.csproj",
        "dotnet/Querify.QnA.Public.Business.Vote/Querify.QnA.Public.Business.Vote.csproj"
    ];

    private static readonly string[] RequiredQnAModelDtoDirectories =
    [
        "dotnet/Querify.Models.QnA/Dtos/Answer",
        "dotnet/Querify.Models.QnA/Dtos/Source",
        "dotnet/Querify.Models.QnA/Dtos/Question",
        "dotnet/Querify.Models.QnA/Dtos/Space",
        "dotnet/Querify.Models.QnA/Dtos/Activity",
        "dotnet/Querify.Models.QnA/Dtos/Tag"
    ];

    private static readonly string[] ProhibitedQnAModelDtoDirectories =
    [
        "dotnet/Querify.Models.QnA/Dtos/Link"
    ];

    private static readonly string[] RequiredQnAModelDtoFiles =
    [
        "dotnet/Querify.Models.QnA/Dtos/Answer/AnswerDto.cs",
        "dotnet/Querify.Models.QnA/Dtos/Answer/AnswerSourceLinkDto.cs",
        "dotnet/Querify.Models.QnA/Dtos/Source/SourceDto.cs",
        "dotnet/Querify.Models.QnA/Dtos/Question/QuestionDto.cs",
        "dotnet/Querify.Models.QnA/Dtos/Question/QuestionSourceLinkDto.cs",
        "dotnet/Querify.Models.QnA/Dtos/Question/QuestionTagDto.cs",
        "dotnet/Querify.Models.QnA/Dtos/Space/SpaceDto.cs",
        "dotnet/Querify.Models.QnA/Dtos/Space/SpaceSourceDto.cs",
        "dotnet/Querify.Models.QnA/Dtos/Space/SpaceTagDto.cs",
        "dotnet/Querify.Models.QnA/Dtos/Activity/ActivityDto.cs",
        "dotnet/Querify.Models.QnA/Dtos/Tag/TagDto.cs"
    ];

    private static readonly HashSet<string> AllowedQnAHelperFiles = new(StringComparer.Ordinal)
    {
        "dotnet/Querify.QnA.Public.Business.Feedback/Helpers/FeedbackRequestContext.cs",
        "dotnet/Querify.QnA.Public.Business.Vote/Helpers/VoteRequestContext.cs"
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

                if (IsAllowedCommandResponseException(filePath, normalizedType))
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

                if (IsAllowedCommandResponseException(filePath, normalizedType))
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

                    if (IsAllowedWriteEndpointResponseException(filePath, attributes, normalizedType))
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
    public void QnABusinessProjects_MustMirrorFeatureBoundaries()
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
    public void QnAModels_MustMirrorDtoFoldersAndFiles()
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

            failures.Add($"{relativePath}: pseudo-entity DTO directories are not allowed in Querify.Models.QnA.");
        }

        var qnaDtosRoot = ToAbsolutePath("dotnet/Querify.Models.QnA/Dtos");
        if (Directory.Exists(qnaDtosRoot))
        {
            foreach (var filePath in Directory.EnumerateFiles(qnaDtosRoot, "*.cs", SearchOption.TopDirectoryOnly))
            {
                failures.Add($"{ToRelativePath(filePath)}: DTO files must live in feature folders, not directly under Dtos.");
            }

            foreach (var filePath in Directory.EnumerateFiles(qnaDtosRoot, "*Dtos.cs", SearchOption.AllDirectories))
            {
                failures.Add($"{ToRelativePath(filePath)}: aggregate *Dtos.cs files are not allowed in Querify.Models.QnA.");
            }
        }

        Assert.True(failures.Count == 0, BuildFailureMessage(failures));
    }

    [Fact]
    public void QnARequestDtos_MustUseAllowedInheritanceOnly()
    {
        var failures = new List<string>();
        var qnaDtosRoot = ToAbsolutePath("dotnet/Querify.Models.QnA/Dtos");

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

            failures.Add($"{ToRelativePath(filePath)}: request DTO inheritance from '{match.Groups["base"].Value}' is not allowed for '{dtoName}' in Querify.Models.QnA.");
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

                failures.Add($"{relativeHelperPath}: generic QnA helper files are not allowed; move logic into commands/queries or use feature-scoped request-context helpers only.");
            }
        }

        Assert.True(failures.Count == 0, BuildFailureMessage(failures));
    }

    [Fact]
    public void QnADomainEntities_MustStayAnemic()
    {
        var failures = new List<string>();
        var entitiesDirectory = ToAbsolutePath("dotnet/Querify.QnA.Common.Domain/Entities");

        foreach (var entityPath in Directory.EnumerateFiles(entitiesDirectory, "*.cs", SearchOption.TopDirectoryOnly))
        {
            var source = File.ReadAllText(entityPath);
            var relativePath = ToRelativePath(entityPath);

            if (source.Contains("[NotMapped]", StringComparison.Ordinal))
            {
                failures.Add($"{relativePath}: [NotMapped] convenience projections are not allowed in QnA domain entities.");
            }

            foreach (Match match in QnAEntityMethodRegex.Matches(source))
            {
                failures.Add($"{relativePath}: behavior method '{match.Value.Trim()}' is not allowed in QnA domain entities.");
            }

            foreach (Match match in QnAEntityConstructorRegex.Matches(source))
            {
                var typeName = Path.GetFileNameWithoutExtension(entityPath);
                var constructorName = match.Groups["name"].Value;
                if (!string.Equals(constructorName, typeName, StringComparison.Ordinal))
                {
                    continue;
                }

                failures.Add($"{relativePath}: constructor '{constructorName}(...)' is not allowed in QnA domain entities.");
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

    private static bool IsAllowedCommandResponseException(string filePath, string normalizedType)
    {
        var key = $"{ToRelativePath(filePath)}|{normalizedType}";
        return AllowedCommandResponseExceptions.Contains(key);
    }

    private static bool IsAllowedWriteEndpointResponseException(
        string filePath,
        string attributes,
        string normalizedType)
    {
        return string.Equals(
                   ToRelativePath(filePath),
                   "dotnet/Querify.QnA.Portal.Business.Source/Controllers/SourceController.cs",
                   StringComparison.Ordinal) &&
               string.Equals(normalizedType, "SourceUploadIntentResponseDto", StringComparison.Ordinal) &&
               attributes.Contains("[HttpPost(\"upload-intent\")]", StringComparison.Ordinal);
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

        return "Repository architecture compliance failures:" + Environment.NewLine +
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
            var solutionPath = Path.Combine(current.FullName, "Querify.sln");
            if (File.Exists(solutionPath))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException("Could not locate repository root containing Querify.sln.");
    }
}
