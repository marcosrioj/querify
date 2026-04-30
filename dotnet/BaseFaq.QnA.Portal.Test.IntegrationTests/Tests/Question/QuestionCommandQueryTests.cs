using System.Net;
using System.Text.Json;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Models.QnA.Dtos.Question;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Portal.Business.Question.Commands.CreateQuestion;
using BaseFaq.QnA.Portal.Business.Question.Commands.DeleteQuestion;
using BaseFaq.QnA.Portal.Business.Question.Commands.UpdateQuestion;
using BaseFaq.QnA.Portal.Business.Question.Queries.GetQuestion;
using BaseFaq.QnA.Portal.Test.IntegrationTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BaseFaq.QnA.Portal.Test.IntegrationTests.Tests.Question;

public class QuestionCommandQueryTests
{
    [Fact]
    public void QuestionContracts_DoNotExposeDuplicateRouting()
    {
        Assert.Null(typeof(QuestionDto).GetProperty("DuplicateOfQuestionId"));
        Assert.Null(
            typeof(QuestionUpdateRequestDto)
                .GetProperty("DuplicateOfQuestionId"));
        Assert.Null(
            typeof(QuestionGetAllRequestDto)
                .GetProperty("DuplicateOfQuestionId"));
        Assert.Null(
            typeof(global::BaseFaq.QnA.Common.Persistence.QnADb.Entities.Question)
                .GetProperty("DuplicateOfQuestionId"));
    }

    [Fact]
    public async Task UpdateQuestion_AcceptsAnswer()
    {
        using var context = TestContext.Create();
        var space = await TestDataFactory.SeedSpaceAsync(context.DbContext, context.SessionService.TenantId);
        var question = await TestDataFactory.SeedQuestionAsync(
            context.DbContext,
            context.SessionService.TenantId,
            space.Id,
            visibility: VisibilityScope.Public);
        var acceptedAnswer = await TestDataFactory.SeedAnswerAsync(
            context.DbContext,
            context.SessionService.TenantId,
            question.Id,
            visibility: VisibilityScope.Public,
            accept: false);
        var updateHandler = new QuestionsUpdateQuestionCommandHandler(
            context.DbContext,
            context.SessionService,
            context.HttpContextAccessor);
        await updateHandler.Handle(new QuestionsUpdateQuestionCommand
        {
            Id = question.Id,
            Request = new QuestionUpdateRequestDto
            {
                Title = question.Title,
                Summary = question.Summary,
                ContextNote = question.ContextNote,
                Status = question.Status,
                Visibility = question.Visibility,
                OriginChannel = question.OriginChannel,
                Sort = question.Sort,
                AcceptedAnswerId = acceptedAnswer.Id
            }
        }, CancellationToken.None);

        var getHandler = new QuestionsGetQuestionQueryHandler(context.DbContext, context.SessionService);
        var result =
            await getHandler.Handle(new QuestionsGetQuestionQuery { Id = question.Id }, CancellationToken.None);

        Assert.Equal(acceptedAnswer.Id, result.AcceptedAnswerId);
        Assert.Equal(QuestionStatus.Active, result.Status);
        Assert.NotNull(result.AcceptedAnswer);
        Assert.True(result.AcceptedAnswer!.IsAccepted);
    }

    [Fact]
    public async Task CreateQuestion_ReturnsApiErrorWhenPublicVisibilityUsesDraftStatus()
    {
        using var context = TestContext.Create();
        var space = await TestDataFactory.SeedSpaceAsync(context.DbContext, context.SessionService.TenantId);
        var createHandler = new QuestionsCreateQuestionCommandHandler(
            context.DbContext,
            context.SessionService,
            context.HttpContextAccessor);

        var exception = await Assert.ThrowsAsync<ApiErrorException>(() => createHandler.Handle(
            new QuestionsCreateQuestionCommand
            {
                Request = new QuestionCreateRequestDto
                {
                    SpaceId = space.Id,
                    Title = "Draft public question",
                    Summary = null,
                    ContextNote = null,
                    Status = QuestionStatus.Draft,
                    Visibility = VisibilityScope.Public,
                    OriginChannel = ChannelKind.Manual,
                    Sort = 0
                }
            },
            CancellationToken.None));

        Assert.Equal((int)HttpStatusCode.UnprocessableEntity, exception.ErrorCode);
    }

    [Fact]
    public async Task CreateQuestion_DefaultsVisibilityToInternalWhenOmitted()
    {
        using var context = TestContext.Create();
        var space = await TestDataFactory.SeedSpaceAsync(context.DbContext, context.SessionService.TenantId);
        var createHandler = new QuestionsCreateQuestionCommandHandler(
            context.DbContext,
            context.SessionService,
            context.HttpContextAccessor);

        var id = await createHandler.Handle(new QuestionsCreateQuestionCommand
        {
            Request = new QuestionCreateRequestDto
            {
                SpaceId = space.Id,
                Title = "Internal default question",
                Summary = null,
                ContextNote = null,
                Status = QuestionStatus.Active,
                OriginChannel = ChannelKind.Manual,
                Sort = 0
            }
        }, CancellationToken.None);

        var getHandler = new QuestionsGetQuestionQueryHandler(context.DbContext, context.SessionService);
        var result =
            await getHandler.Handle(new QuestionsGetQuestionQuery { Id = id }, CancellationToken.None);

        Assert.Equal(VisibilityScope.Internal, result.Visibility);
    }

    [Fact]
    public async Task CreateQuestion_AppendsCreatedActivityWithMetadata()
    {
        using var context = TestContext.Create();
        var space = await TestDataFactory.SeedSpaceAsync(context.DbContext, context.SessionService.TenantId);
        var createHandler = new QuestionsCreateQuestionCommandHandler(
            context.DbContext,
            context.SessionService,
            context.HttpContextAccessor);

        var id = await createHandler.Handle(new QuestionsCreateQuestionCommand
        {
            Request = new QuestionCreateRequestDto
            {
                SpaceId = space.Id,
                Title = "How do I rotate API keys?",
                Summary = "Rotate API keys",
                ContextNote = "Security operations",
                Status = QuestionStatus.Active,
                Visibility = VisibilityScope.Internal,
                OriginChannel = ChannelKind.Manual,
                Sort = 7
            }
        }, CancellationToken.None);

        var activities = await context.DbContext.Activities
            .Where(activity => activity.QuestionId == id)
            .ToListAsync();
        var createdActivity = Assert.Single(activities);
        Assert.Equal(ActivityKind.QuestionCreated, createdActivity.Kind);
        Assert.Contains(context.SessionService.UserName!, createdActivity.Notes);

        using var createdMetadata = JsonDocument.Parse(createdActivity.MetadataJson!);
        Assert.Equal("Question", createdMetadata.RootElement.GetProperty("Entity").GetString());
        Assert.Equal("Created", createdMetadata.RootElement.GetProperty("Operation").GetString());
        Assert.Equal(
            context.SessionService.UserId.ToString("D"),
            createdMetadata.RootElement
                .GetProperty("Context")
                .GetProperty("ActorUserId")
                .GetString());
        Assert.Equal(
            context.SessionService.UserName,
            createdMetadata.RootElement
                .GetProperty("Context")
                .GetProperty("ActorUserName")
                .GetString());
        Assert.Equal(
            "How do I rotate API keys?",
            createdMetadata.RootElement
                .GetProperty("Changes")
                .GetProperty("Title")
                .GetProperty("After")
                .GetString());
        Assert.Equal(
            "Active",
            createdMetadata.RootElement
                .GetProperty("Changes")
                .GetProperty("Status")
                .GetProperty("After")
                .GetString());
    }

    [Fact]
    public async Task UpdateQuestion_AppendsUpdatedActivityWhenStatusDoesNotChange()
    {
        using var context = TestContext.Create();
        var space = await TestDataFactory.SeedSpaceAsync(context.DbContext, context.SessionService.TenantId);
        var question = await TestDataFactory.SeedQuestionAsync(
            context.DbContext,
            context.SessionService.TenantId,
            space.Id,
            status: QuestionStatus.Active,
            visibility: VisibilityScope.Internal);
        var updateHandler = new QuestionsUpdateQuestionCommandHandler(
            context.DbContext,
            context.SessionService,
            context.HttpContextAccessor);

        await updateHandler.Handle(new QuestionsUpdateQuestionCommand
        {
            Id = question.Id,
            Request = new QuestionUpdateRequestDto
            {
                Title = "How do I rotate API keys safely?",
                Summary = question.Summary,
                ContextNote = question.ContextNote,
                Status = question.Status,
                Visibility = VisibilityScope.Internal,
                OriginChannel = question.OriginChannel,
                Sort = question.Sort,
                AcceptedAnswerId = question.AcceptedAnswerId
            }
        }, CancellationToken.None);

        var updatedActivity = await context.DbContext.Activities
            .SingleAsync(activity => activity.QuestionId == question.Id && activity.Kind == ActivityKind.QuestionUpdated);
        Assert.False(await context.DbContext.Activities
            .AnyAsync(activity => activity.QuestionId == question.Id && activity.Kind == ActivityKind.QuestionArchived));
        Assert.Contains(context.SessionService.UserName!, updatedActivity.Notes);

        using var updatedMetadata = JsonDocument.Parse(updatedActivity.MetadataJson!);
        var updatedFields = updatedMetadata.RootElement
            .GetProperty("ChangedFields")
            .EnumerateArray()
            .Select(field => field.GetString())
            .ToList();
        Assert.Contains("Title", updatedFields);
        Assert.DoesNotContain("Status", updatedFields);
        Assert.Equal(
            "How do I rotate API keys safely?",
            updatedMetadata.RootElement
                .GetProperty("Changes")
                .GetProperty("Title")
                .GetProperty("After")
                .GetString());
    }

    [Fact]
    public async Task UpdateQuestion_UsesStatusActivityWhenStatusChanges()
    {
        using var context = TestContext.Create();
        var space = await TestDataFactory.SeedSpaceAsync(context.DbContext, context.SessionService.TenantId);
        var question = await TestDataFactory.SeedQuestionAsync(
            context.DbContext,
            context.SessionService.TenantId,
            space.Id,
            status: QuestionStatus.Active,
            visibility: VisibilityScope.Internal);
        var updateHandler = new QuestionsUpdateQuestionCommandHandler(
            context.DbContext,
            context.SessionService,
            context.HttpContextAccessor);

        await updateHandler.Handle(new QuestionsUpdateQuestionCommand
        {
            Id = question.Id,
            Request = new QuestionUpdateRequestDto
            {
                Title = "How do I rotate API keys safely?",
                Summary = question.Summary,
                ContextNote = question.ContextNote,
                Status = QuestionStatus.Archived,
                Visibility = VisibilityScope.Internal,
                OriginChannel = question.OriginChannel,
                Sort = question.Sort,
                AcceptedAnswerId = question.AcceptedAnswerId
            }
        }, CancellationToken.None);

        Assert.False(await context.DbContext.Activities
            .AnyAsync(activity => activity.QuestionId == question.Id && activity.Kind == ActivityKind.QuestionUpdated));
        var archivedActivity = await context.DbContext.Activities
            .SingleAsync(activity => activity.QuestionId == question.Id && activity.Kind == ActivityKind.QuestionArchived);
        Assert.Contains(context.SessionService.UserName!, archivedActivity.Notes);

        using var archivedMetadata = JsonDocument.Parse(archivedActivity.MetadataJson!);
        var archivedFields = archivedMetadata.RootElement
            .GetProperty("ChangedFields")
            .EnumerateArray()
            .Select(field => field.GetString())
            .ToList();
        Assert.Contains("Title", archivedFields);
        Assert.Contains("Status", archivedFields);
        Assert.Equal("StatusChanged", archivedMetadata.RootElement.GetProperty("Operation").GetString());
        Assert.Equal(
            "Archived",
            archivedMetadata.RootElement
                .GetProperty("Changes")
                .GetProperty("Status")
                .GetProperty("After")
                .GetString());
    }

    [Fact]
    public async Task CreateQuestion_ReturnsApiErrorWhenStatusIsUnsupportedLegacyValue()
    {
        using var context = TestContext.Create();
        var space = await TestDataFactory.SeedSpaceAsync(context.DbContext, context.SessionService.TenantId);
        var createHandler = new QuestionsCreateQuestionCommandHandler(
            context.DbContext,
            context.SessionService,
            context.HttpContextAccessor);

        var exception = await Assert.ThrowsAsync<ApiErrorException>(() => createHandler.Handle(
            new QuestionsCreateQuestionCommand
            {
                Request = new QuestionCreateRequestDto
                {
                    SpaceId = space.Id,
                    Title = "Unsupported legacy status",
                    Summary = null,
                    ContextNote = null,
                    Status = (QuestionStatus)99,
                    Visibility = VisibilityScope.Internal,
                    OriginChannel = ChannelKind.Manual,
                    Sort = 0
                }
            },
            CancellationToken.None));

        Assert.Equal((int)HttpStatusCode.UnprocessableEntity, exception.ErrorCode);
        Assert.Equal("Unsupported question status.", exception.Message);
    }

    [Fact]
    public async Task DeleteQuestion_SoftDeletesAnswers()
    {
        using var context = TestContext.Create();
        var space = await TestDataFactory.SeedSpaceAsync(context.DbContext, context.SessionService.TenantId);
        var question = await TestDataFactory.SeedQuestionAsync(
            context.DbContext,
            context.SessionService.TenantId,
            space.Id);
        var answer = await TestDataFactory.SeedAnswerAsync(
            context.DbContext,
            context.SessionService.TenantId,
            question.Id);
        var otherQuestion = await TestDataFactory.SeedQuestionAsync(
            context.DbContext,
            context.SessionService.TenantId,
            space.Id,
            title: "How do I update billing?");
        var otherAnswer = await TestDataFactory.SeedAnswerAsync(
            context.DbContext,
            context.SessionService.TenantId,
            otherQuestion.Id,
            headline: "Open billing settings.");

        var deleteHandler = new QuestionsDeleteQuestionCommandHandler(context.DbContext, context.SessionService);
        await deleteHandler.Handle(new QuestionsDeleteQuestionCommand { Id = question.Id }, CancellationToken.None);

        context.DbContext.ChangeTracker.Clear();

        Assert.False(await context.DbContext.Questions.AnyAsync(entity => entity.Id == question.Id));
        Assert.False(await context.DbContext.Answers.AnyAsync(entity => entity.Id == answer.Id));
        Assert.True(await context.DbContext.Questions.AnyAsync(entity => entity.Id == otherQuestion.Id));
        Assert.True(await context.DbContext.Answers.AnyAsync(entity => entity.Id == otherAnswer.Id));

        context.DbContext.SoftDeleteFiltersEnabled = false;
        var deletedQuestion = await context.DbContext.Questions.SingleAsync(entity => entity.Id == question.Id);
        var deletedAnswer = await context.DbContext.Answers.SingleAsync(entity => entity.Id == answer.Id);
        var retainedQuestion = await context.DbContext.Questions.SingleAsync(entity => entity.Id == otherQuestion.Id);
        var retainedAnswer = await context.DbContext.Answers.SingleAsync(entity => entity.Id == otherAnswer.Id);

        Assert.True(deletedQuestion.IsDeleted);
        Assert.True(deletedAnswer.IsDeleted);
        Assert.False(retainedQuestion.IsDeleted);
        Assert.False(retainedAnswer.IsDeleted);
    }
}
