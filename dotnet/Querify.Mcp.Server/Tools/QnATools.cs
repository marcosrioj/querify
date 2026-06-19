using System.ComponentModel;
using MediatR;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;
using Querify.Mcp.Common.Constants;
using Querify.Mcp.Common.Results;
using Querify.Mcp.Common.Security;
using Querify.Mcp.Server.Infrastructure;
using Querify.Mcp.Server.Options;
using Querify.Models.QnA.Dtos.Answer;
using Querify.Models.QnA.Dtos.Question;
using Querify.Models.QnA.Dtos.Source;
using Querify.Models.QnA.Dtos.Space;
using Querify.Models.QnA.Enums;
using Querify.QnA.Portal.Business.Answer.Commands.ActivateAnswer;
using Querify.QnA.Portal.Business.Answer.Commands.AddSource;
using Querify.QnA.Portal.Business.Answer.Commands.CreateAnswer;
using Querify.QnA.Portal.Business.Question.Commands.AddSource;
using Querify.QnA.Portal.Business.Question.Commands.CreateQuestion;
using Querify.QnA.Portal.Business.Question.Queries.GetQuestion;
using Querify.QnA.Portal.Business.Question.Queries.GetQuestionList;
using Querify.QnA.Portal.Business.Source.Commands.CreateSource;
using Querify.QnA.Portal.Business.Source.Queries.GetSource;
using Querify.QnA.Portal.Business.Source.Queries.GetSourceList;
using Querify.QnA.Portal.Business.Space.Queries.GetSpace;
using Querify.QnA.Portal.Business.Space.Queries.GetSpaceList;
using QuerifyMcpServerOptions = Querify.Mcp.Server.Options.McpServerOptions;

namespace Querify.Mcp.Server.Tools;

[McpServerToolType]
public sealed class QnATools(
    IMediator mediator,
    McpRequestContext requestContext,
    IOptions<QuerifyMcpServerOptions> options)
{
    [McpServerTool(
        Name = McpToolNames.QnAListSpaces,
        ReadOnly = true,
        Destructive = false,
        OpenWorld = false)]
    [Description("Reads QnA spaces for a tenant through the QnA Space query boundary.")]
    public Task<string> ListSpaces(
        [Description("Tenant id. When omitted, McpServer:DefaultTenantId is used.")]
        Guid? tenantId = null,
        [Description("Optional text filter for space name, slug, summary, or language.")]
        string? searchText = null,
        [Description("Zero-based item offset.")]
        int skipCount = 0,
        [Description("Requested page size. The server caps this with McpServer:ToolResultMaxItems.")]
        int maxResultCount = 20,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(tenantId, async () =>
            await mediator.Send(new SpacesGetSpaceListQuery
            {
                Request = new SpaceGetAllRequestDto
                {
                    SearchText = searchText,
                    SkipCount = NormalizeSkip(skipCount),
                    MaxResultCount = NormalizeTake(maxResultCount)
                }
            }, cancellationToken));
    }

    [McpServerTool(
        Name = McpToolNames.QnAGetSpace,
        ReadOnly = true,
        Destructive = false,
        OpenWorld = false)]
    [Description("Reads one QnA space by id through the QnA Space query boundary.")]
    public Task<string> GetSpace(
        [Description("Space id.")]
        Guid id,
        [Description("Tenant id. When omitted, McpServer:DefaultTenantId is used.")]
        Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(tenantId, async () =>
            await mediator.Send(new SpacesGetSpaceQuery { Id = id }, cancellationToken));
    }

    [McpServerTool(
        Name = McpToolNames.QnAListQuestions,
        ReadOnly = true,
        Destructive = false,
        OpenWorld = false)]
    [Description("Reads QnA questions for a tenant through the QnA Question query boundary.")]
    public Task<string> ListQuestions(
        [Description("Tenant id. When omitted, McpServer:DefaultTenantId is used.")]
        Guid? tenantId = null,
        [Description("Optional space filter.")]
        Guid? spaceId = null,
        [Description("Optional source filter.")]
        Guid? sourceId = null,
        [Description("Optional text filter.")]
        string? searchText = null,
        [Description("Zero-based item offset.")]
        int skipCount = 0,
        [Description("Requested page size. The server caps this with McpServer:ToolResultMaxItems.")]
        int maxResultCount = 20,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(tenantId, async () =>
            await mediator.Send(new QuestionsGetQuestionListQuery
            {
                Request = new QuestionGetAllRequestDto
                {
                    SpaceId = spaceId,
                    SourceId = sourceId,
                    SearchText = searchText,
                    SkipCount = NormalizeSkip(skipCount),
                    MaxResultCount = NormalizeTake(maxResultCount)
                }
            }, cancellationToken));
    }

    [McpServerTool(
        Name = McpToolNames.QnAGetQuestion,
        ReadOnly = true,
        Destructive = false,
        OpenWorld = false)]
    [Description("Reads one QnA question by id through the QnA Question query boundary.")]
    public Task<string> GetQuestion(
        [Description("Question id.")]
        Guid id,
        [Description("Tenant id. When omitted, McpServer:DefaultTenantId is used.")]
        Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(tenantId, async () =>
            await mediator.Send(new QuestionsGetQuestionQuery { Id = id }, cancellationToken));
    }

    [McpServerTool(
        Name = McpToolNames.QnAListSources,
        ReadOnly = true,
        Destructive = false,
        OpenWorld = false)]
    [Description("Reads QnA sources for a tenant through the QnA Source query boundary.")]
    public Task<string> ListSources(
        [Description("Tenant id. When omitted, McpServer:DefaultTenantId is used.")]
        Guid? tenantId = null,
        [Description("Optional text filter.")]
        string? searchText = null,
        [Description("Zero-based item offset.")]
        int skipCount = 0,
        [Description("Requested page size. The server caps this with McpServer:ToolResultMaxItems.")]
        int maxResultCount = 20,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(tenantId, async () =>
            await mediator.Send(new SourcesGetSourceListQuery
            {
                Request = new SourceGetAllRequestDto
                {
                    SearchText = searchText,
                    SkipCount = NormalizeSkip(skipCount),
                    MaxResultCount = NormalizeTake(maxResultCount)
                }
            }, cancellationToken));
    }

    [McpServerTool(
        Name = McpToolNames.QnAGetSource,
        ReadOnly = true,
        Destructive = false,
        OpenWorld = false)]
    [Description("Reads one QnA source by id through the QnA Source query boundary.")]
    public Task<string> GetSource(
        [Description("Source id.")]
        Guid id,
        [Description("Tenant id. When omitted, McpServer:DefaultTenantId is used.")]
        Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(tenantId, async () =>
            await mediator.Send(new SourcesGetSourceQuery { Id = id }, cancellationToken));
    }

    [McpServerTool(
        Name = McpToolNames.QnACreateQuestion,
        ReadOnly = false,
        Destructive = false,
        OpenWorld = false)]
    [Description("Creates a Draft/Internal QnA question through the QnA Question command boundary. Requires write tools to be enabled.")]
    public Task<string> CreateQuestion(
        [Description("Space id that will own the question.")]
        Guid spaceId,
        [Description("Question title.")]
        string title,
        [Description("Tenant id. When omitted, McpServer:DefaultTenantId is used.")]
        Guid? tenantId = null,
        [Description("Optional short summary.")]
        string? summary = null,
        [Description("Optional operator note.")]
        string? contextNote = null,
        [Description("Sort order within the space or follow-up branch.")]
        int sort = 0,
        [Description("Parent answer id when creating a follow-up question.")]
        Guid? parentAnswerId = null,
        CancellationToken cancellationToken = default)
    {
        return ExecuteWriteAsync(McpToolNames.QnACreateQuestion, tenantId, async () =>
        {
            var id = await mediator.Send(new QuestionsCreateQuestionCommand
            {
                Request = new QuestionCreateRequestDto
                {
                    SpaceId = spaceId,
                    Title = title,
                    Summary = summary,
                    ContextNote = contextNote,
                    Status = QuestionStatus.Draft,
                    Visibility = VisibilityScope.Internal,
                    OriginChannel = ChannelKind.Api,
                    Sort = sort,
                    ParentAnswerId = parentAnswerId
                }
            }, cancellationToken);

            return new McpWriteResult(id);
        });
    }

    [McpServerTool(
        Name = McpToolNames.QnACreateAnswer,
        ReadOnly = false,
        Destructive = false,
        OpenWorld = false)]
    [Description("Creates a Draft/Internal QnA answer through the QnA Answer command boundary. Requires write tools to be enabled.")]
    public Task<string> CreateAnswer(
        [Description("Question id that will own the answer.")]
        Guid questionId,
        [Description("Answer headline.")]
        string headline,
        [Description("Tenant id. When omitted, McpServer:DefaultTenantId is used.")]
        Guid? tenantId = null,
        [Description("Optional answer body.")]
        string? body = null,
        [Description("Optional operator note.")]
        string? contextNote = null,
        [Description("Optional author label.")]
        string? authorLabel = null,
        [Description("Sort order within the parent question.")]
        int sort = 0,
        [Description("Optional follow-up question ids supported by this answer.")]
        IReadOnlyList<Guid>? followUpQuestionIds = null,
        CancellationToken cancellationToken = default)
    {
        return ExecuteWriteAsync(McpToolNames.QnACreateAnswer, tenantId, async () =>
        {
            var id = await mediator.Send(new AnswersCreateAnswerCommand
            {
                Request = new AnswerCreateRequestDto
                {
                    QuestionId = questionId,
                    Headline = headline,
                    Body = body,
                    Kind = AnswerKind.Imported,
                    Status = AnswerStatus.Draft,
                    Visibility = VisibilityScope.Internal,
                    ContextNote = contextNote,
                    AuthorLabel = authorLabel,
                    Sort = sort,
                    FollowUpQuestionIds = followUpQuestionIds ?? []
                }
            }, cancellationToken);

            return new McpWriteResult(id);
        });
    }

    [McpServerTool(
        Name = McpToolNames.QnAActivateAnswer,
        ReadOnly = false,
        Destructive = false,
        OpenWorld = false)]
    [Description("Activates a QnA answer through the QnA Answer command boundary. Requires write tools to be enabled.")]
    public Task<string> ActivateAnswer(
        [Description("Answer id.")]
        Guid answerId,
        [Description("Tenant id. When omitted, McpServer:DefaultTenantId is used.")]
        Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        return ExecuteWriteAsync(McpToolNames.QnAActivateAnswer, tenantId, async () =>
        {
            var id = await mediator.Send(new AnswersActivateAnswerCommand { Id = answerId }, cancellationToken);
            return new McpWriteResult(id);
        });
    }

    [McpServerTool(
        Name = McpToolNames.QnACreateSource,
        ReadOnly = false,
        Destructive = false,
        OpenWorld = false)]
    [Description("Creates a QnA source through the QnA Source command boundary. Requires write tools to be enabled.")]
    public Task<string> CreateSource(
        [Description("Source locator, such as an HTTP URL or storage locator.")]
        string locator,
        [Description("Source language code.")]
        string language = "en",
        [Description("Tenant id. When omitted, McpServer:DefaultTenantId is used.")]
        Guid? tenantId = null,
        [Description("Optional source label.")]
        string? label = null,
        [Description("Optional operator note.")]
        string? contextNote = null,
        [Description("Optional external source id.")]
        string? externalId = null,
        [Description("Optional media type.")]
        string? mediaType = null,
        [Description("Optional metadata JSON.")]
        string? metadataJson = null,
        CancellationToken cancellationToken = default)
    {
        return ExecuteWriteAsync(McpToolNames.QnACreateSource, tenantId, async () =>
        {
            var id = await mediator.Send(new SourcesCreateSourceCommand
            {
                Request = new SourceCreateRequestDto
                {
                    Locator = locator,
                    Label = label,
                    ContextNote = contextNote,
                    ExternalId = externalId,
                    Language = language,
                    MediaType = mediaType,
                    MetadataJson = metadataJson
                }
            }, cancellationToken);

            return new McpWriteResult(id);
        });
    }

    [McpServerTool(
        Name = McpToolNames.QnALinkQuestionSource,
        ReadOnly = false,
        Destructive = false,
        OpenWorld = false)]
    [Description("Links an existing source to a QnA question through the QnA Question command boundary. Requires write tools to be enabled.")]
    public Task<string> LinkQuestionSource(
        [Description("Question id.")]
        Guid questionId,
        [Description("Source id.")]
        Guid sourceId,
        [Description("Tenant id. When omitted, McpServer:DefaultTenantId is used.")]
        Guid? tenantId = null,
        [Description("Source role.")]
        SourceRole role = SourceRole.Evidence,
        [Description("Display order for the source link.")]
        int order = 0,
        CancellationToken cancellationToken = default)
    {
        return ExecuteWriteAsync(McpToolNames.QnALinkQuestionSource, tenantId, async () =>
        {
            var id = await mediator.Send(new QuestionsAddSourceCommand
            {
                Request = new QuestionSourceLinkCreateRequestDto
                {
                    QuestionId = questionId,
                    SourceId = sourceId,
                    Role = role,
                    Order = order
                }
            }, cancellationToken);

            return new McpWriteResult(id);
        });
    }

    [McpServerTool(
        Name = McpToolNames.QnALinkAnswerSource,
        ReadOnly = false,
        Destructive = false,
        OpenWorld = false)]
    [Description("Links an existing source to a QnA answer through the QnA Answer command boundary. Requires write tools to be enabled.")]
    public Task<string> LinkAnswerSource(
        [Description("Answer id.")]
        Guid answerId,
        [Description("Source id.")]
        Guid sourceId,
        [Description("Tenant id. When omitted, McpServer:DefaultTenantId is used.")]
        Guid? tenantId = null,
        [Description("Source role.")]
        SourceRole role = SourceRole.Evidence,
        [Description("Display order for the source link.")]
        int order = 0,
        CancellationToken cancellationToken = default)
    {
        return ExecuteWriteAsync(McpToolNames.QnALinkAnswerSource, tenantId, async () =>
        {
            var id = await mediator.Send(new AnswersAddSourceCommand
            {
                Request = new AnswerSourceLinkCreateRequestDto
                {
                    AnswerId = answerId,
                    SourceId = sourceId,
                    Role = role,
                    Order = order
                }
            }, cancellationToken);

            return new McpWriteResult(id);
        });
    }

    private Task<string> ExecuteAsync(Guid? tenantId, Func<Task<object?>> action)
    {
        return McpToolExecution.ExecuteAsync(async () =>
        {
            requestContext.Configure(tenantId);
            return await action();
        });
    }

    private Task<string> ExecuteWriteAsync(string toolName, Guid? tenantId, Func<Task<object?>> action)
    {
        return McpToolExecution.ExecuteAsync(async () =>
        {
            McpToolAuthorization.EnsureWriteToolsEnabled(options.Value.EnableWriteTools, toolName);
            requestContext.Configure(tenantId);
            return await action();
        });
    }

    private int NormalizeSkip(int skipCount)
    {
        return Math.Max(0, skipCount);
    }

    private int NormalizeTake(int maxResultCount)
    {
        var configuredLimit = options.Value.ToolResultMaxItems <= 0 ? 20 : options.Value.ToolResultMaxItems;
        var requested = maxResultCount <= 0 ? configuredLimit : maxResultCount;
        return Math.Clamp(requested, 1, configuredLimit);
    }
}
