using Microsoft.EntityFrameworkCore;
using Querify.Models.QnA.Dtos.SourceGeneration;
using Querify.Models.QnA.Enums;
using Querify.QnA.Portal.Business.SourceGeneration.Commands.CreateSpaceGenerationRun;
using Querify.QnA.Portal.Business.SourceGeneration.Queries.GetSpaceGenerationRun;
using Querify.QnA.Portal.Business.SourceGeneration.Queries.GetSpaceGenerationRunList;
using Querify.QnA.Portal.Test.IntegrationTests.Helpers;
using Querify.QnA.Worker.Business.SourceGeneration.Commands.ExecuteSpaceGenerationRun;
using Xunit;

namespace Querify.QnA.Portal.Test.IntegrationTests.Tests.SourceGeneration;

public sealed class SourceGenerationCommandQueryTests
{
    [Fact]
    public async Task GenerateSpaceFromSource_CreatesDraftGraphAndCompletedRun()
    {
        using var context = TestContext.Create();
        var source = await TestDataFactory.SeedSourceAsync(context.DbContext, context.SessionService.TenantId);
        var createHandler = new SourcesCreateSpaceGenerationRunCommandHandler(
            context.DbContext,
            context.SessionService);
        var executeHandler = new SourcesExecuteSpaceGenerationRunCommandHandler(
            context.DbContext,
            context.SessionService);

        var runId = await createHandler.Handle(new SourcesCreateSpaceGenerationRunCommand
        {
            SourceId = source.Id,
            Request = new SourceGenerateSpaceRequestDto
            {
                ExtractionGoal = "Create operator-reviewed support content.",
                ContentHint = "Use the setup and troubleshooting sections."
            }
        }, CancellationToken.None);

        var commandResult = await executeHandler.Handle(new SourcesExecuteSpaceGenerationRunCommand
        {
            RunId = runId
        }, CancellationToken.None);

        var getHandler = new SourcesGetSpaceGenerationRunQueryHandler(
            context.DbContext,
            context.SessionService);
        var run = await getHandler.Handle(new SourcesGetSpaceGenerationRunQuery { RunId = runId },
            CancellationToken.None);

        Assert.Equal(run.CreatedSpaceId, commandResult);
        Assert.Equal(SourceGenerationRunStatus.Completed, run.Status);
        Assert.Equal(source.Id, run.SourceId);
        Assert.Null(run.FailureReason);
        Assert.NotNull(run.Warning);
        Assert.Equal("Reset Password Doc", run.SpaceName);
        Assert.Equal("reset-password-doc", run.SpaceSlug);
        Assert.Equal(SourceRole.Evidence, run.SourceRole);
        Assert.True(run.RequireEveryAnswerToCiteSource);
        Assert.Equal(3, run.MaxTopLevelQuestions);
        Assert.Equal(2, run.MaxFollowUpDepth);
        Assert.Equal(2, run.MaxAnswersPerQuestion);
        Assert.True(run.IncludeFollowUpQuestions);

        var space = await context.DbContext.Spaces
            .Include(entity => entity.Sources)
            .Include(entity => entity.Tags)
            .SingleAsync(entity => entity.Id == run.CreatedSpaceId);
        Assert.Equal("Reset Password Doc", space.Name);
        Assert.Equal("reset-password-doc", space.Slug);
        Assert.Equal(SpaceStatus.Draft, space.Status);
        Assert.Equal(VisibilityScope.Internal, space.Visibility);
        Assert.Contains(space.Sources, link => link.SourceId == source.Id && link.Role == SourceRole.Evidence);
        Assert.NotEmpty(space.Tags);

        var questions = await context.DbContext.Questions
            .Include(entity => entity.Answers)
            .Include(entity => entity.Sources)
            .Include(entity => entity.Tags)
            .Where(entity => entity.SpaceId == space.Id)
            .ToListAsync();
        Assert.Equal(9, questions.Count);
        Assert.Contains(questions, question => question.ParentAnswerId.HasValue);
        Assert.All(questions, question =>
        {
            Assert.Equal(QuestionStatus.Draft, question.Status);
            Assert.Equal(VisibilityScope.Internal, question.Visibility);
            Assert.Equal(ChannelKind.Import, question.OriginChannel);
            Assert.Contains(question.Sources, link => link.SourceId == source.Id && link.Role == SourceRole.Evidence);
            Assert.NotEmpty(question.Tags);
        });

        var answers = await context.DbContext.Answers
            .Include(entity => entity.Sources)
            .Where(entity => questions.Select(question => question.Id).Contains(entity.QuestionId))
            .ToListAsync();
        Assert.Equal(12, answers.Count);
        Assert.All(answers, answer =>
        {
            Assert.Equal(AnswerStatus.Draft, answer.Status);
            Assert.Equal(VisibilityScope.Internal, answer.Visibility);
            Assert.Equal(AnswerKind.Imported, answer.Kind);
            Assert.Contains(answer.Sources, link => link.SourceId == source.Id && link.Role == SourceRole.Evidence);
        });
    }

    [Fact]
    public async Task GetSpaceGenerationRunList_ReturnsSourceScopedRuns()
    {
        using var context = TestContext.Create();
        var source = await TestDataFactory.SeedSourceAsync(context.DbContext, context.SessionService.TenantId);
        var createHandler = new SourcesCreateSpaceGenerationRunCommandHandler(
            context.DbContext,
            context.SessionService);
        var runId = await createHandler.Handle(new SourcesCreateSpaceGenerationRunCommand
        {
            SourceId = source.Id,
            Request = new SourceGenerateSpaceRequestDto
            {
                ExtractionGoal = "Prioritize operator support use cases."
            }
        }, CancellationToken.None);

        var listHandler = new SourcesGetSpaceGenerationRunListQueryHandler(
            context.DbContext,
            context.SessionService);
        var result = await listHandler.Handle(new SourcesGetSpaceGenerationRunListQuery
        {
            SourceId = source.Id,
            MaxResultCount = 20
        }, CancellationToken.None);

        var item = Assert.Single(result.Items);
        Assert.Equal(runId, item.Id);
        Assert.Equal(SourceGenerationRunStatus.Pending, item.Status);
        Assert.Equal(SourceGenerationTagMode.CreateAndAttach, item.TagGenerationMode);
    }
}
