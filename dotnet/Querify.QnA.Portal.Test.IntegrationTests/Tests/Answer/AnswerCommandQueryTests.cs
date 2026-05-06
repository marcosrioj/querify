using System.Net;
using System.Text.Json;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Models.QnA.Dtos.Answer;
using Querify.Models.QnA.Enums;
using Querify.QnA.Portal.Business.Answer.Commands.CreateAnswer;
using Querify.QnA.Portal.Business.Answer.Commands.UpdateAnswer;
using Querify.QnA.Portal.Business.Answer.Queries.GetAnswer;
using Querify.QnA.Portal.Test.IntegrationTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Querify.QnA.Portal.Test.IntegrationTests.Tests.Answer;

public class AnswerCommandQueryTests
{
    [Fact]
    public async Task CreateAnswer_PersistsEntityAndReturnsDto()
    {
        using var context = TestContext.Create();
        var space = await TestDataFactory.SeedSpaceAsync(context.DbContext, context.SessionService.TenantId);
        var question =
            await TestDataFactory.SeedQuestionAsync(context.DbContext, context.SessionService.TenantId, space.Id);

        var createHandler = new AnswersCreateAnswerCommandHandler(
            context.DbContext,
            context.SessionService,
            context.HttpContextAccessor);
        var id = await createHandler.Handle(new AnswersCreateAnswerCommand
        {
            Request = new AnswerCreateRequestDto
            {
                QuestionId = question.Id,
                Headline = "Reset password from portal",
                Body = "Open sign-in and request a password reset link.",
                Kind = AnswerKind.Official,
                Status = AnswerStatus.Active,
                Visibility = VisibilityScope.Public,
                ContextNote = "Portal",
                Sort = 2
            }
        }, CancellationToken.None);

        var getHandler = new AnswersGetAnswerQueryHandler(context.DbContext, context.SessionService);
        var result = await getHandler.Handle(new AnswersGetAnswerQuery { Id = id }, CancellationToken.None);

        Assert.Equal(question.Id, result.QuestionId);
        Assert.Equal("Reset password from portal", result.Headline);
        Assert.Equal(AnswerStatus.Active, result.Status);
        Assert.Equal(VisibilityScope.Public, result.Visibility);

        var activities = await context.DbContext.Activities
            .Where(activity => activity.AnswerId == id)
            .ToListAsync();
        var createdActivity = Assert.Single(activities);
        Assert.Equal(ActivityKind.AnswerCreated, createdActivity.Kind);
        Assert.Contains(context.SessionService.UserName!, createdActivity.Notes);

        using var createdMetadata = JsonDocument.Parse(createdActivity.MetadataJson!);
        Assert.Equal("Answer", createdMetadata.RootElement.GetProperty("Entity").GetString());
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
            "Reset password from portal",
            createdMetadata.RootElement
                .GetProperty("Changes")
                .GetProperty("Headline")
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
    public async Task CreateAnswer_ReturnsApiErrorWhenPublicVisibilityUsesDraftStatus()
    {
        using var context = TestContext.Create();
        var space = await TestDataFactory.SeedSpaceAsync(context.DbContext, context.SessionService.TenantId);
        var question =
            await TestDataFactory.SeedQuestionAsync(context.DbContext, context.SessionService.TenantId, space.Id);

        var createHandler = new AnswersCreateAnswerCommandHandler(
            context.DbContext,
            context.SessionService,
            context.HttpContextAccessor);

        var exception = await Assert.ThrowsAsync<ApiErrorException>(() => createHandler.Handle(
            new AnswersCreateAnswerCommand
            {
                Request = new AnswerCreateRequestDto
                {
                    QuestionId = question.Id,
                    Headline = "Draft public answer",
                    Body = "Draft body",
                    Kind = AnswerKind.Official,
                    Status = AnswerStatus.Draft,
                    Visibility = VisibilityScope.Public,
                    ContextNote = null,
                    Sort = 0
                }
            },
            CancellationToken.None));

        Assert.Equal((int)HttpStatusCode.UnprocessableEntity, exception.ErrorCode);
    }

    [Fact]
    public async Task CreateAnswer_DefaultsVisibilityToInternalWhenOmitted()
    {
        using var context = TestContext.Create();
        var space = await TestDataFactory.SeedSpaceAsync(context.DbContext, context.SessionService.TenantId);
        var question =
            await TestDataFactory.SeedQuestionAsync(context.DbContext, context.SessionService.TenantId, space.Id);

        var createHandler = new AnswersCreateAnswerCommandHandler(
            context.DbContext,
            context.SessionService,
            context.HttpContextAccessor);
        var id = await createHandler.Handle(new AnswersCreateAnswerCommand
        {
            Request = new AnswerCreateRequestDto
            {
                QuestionId = question.Id,
                Headline = "Internal default answer",
                Body = "Internal answer body",
                Kind = AnswerKind.Official,
                Status = AnswerStatus.Active,
                ContextNote = null,
                Sort = 0
            }
        }, CancellationToken.None);

        var getHandler = new AnswersGetAnswerQueryHandler(context.DbContext, context.SessionService);
        var result = await getHandler.Handle(new AnswersGetAnswerQuery { Id = id }, CancellationToken.None);

        Assert.Equal(VisibilityScope.Internal, result.Visibility);
    }

    [Fact]
    public async Task CreateAnswer_ReturnsApiErrorWhenStatusIsUnsupported()
    {
        using var context = TestContext.Create();
        var space = await TestDataFactory.SeedSpaceAsync(context.DbContext, context.SessionService.TenantId);
        var question =
            await TestDataFactory.SeedQuestionAsync(context.DbContext, context.SessionService.TenantId, space.Id);

        var createHandler = new AnswersCreateAnswerCommandHandler(
            context.DbContext,
            context.SessionService,
            context.HttpContextAccessor);

        var exception = await Assert.ThrowsAsync<ApiErrorException>(() => createHandler.Handle(
            new AnswersCreateAnswerCommand
            {
                Request = new AnswerCreateRequestDto
                {
                    QuestionId = question.Id,
                    Headline = "Legacy answer status",
                    Body = "Legacy body",
                    Kind = AnswerKind.Official,
                    Status = (AnswerStatus)3,
                    Visibility = VisibilityScope.Internal,
                    ContextNote = null,
                    Sort = 0
                }
            },
            CancellationToken.None));

        Assert.Equal((int)HttpStatusCode.UnprocessableEntity, exception.ErrorCode);
    }

    [Fact]
    public async Task UpdateAnswer_AppendsUpdatedActivityWhenStatusDoesNotChange()
    {
        using var context = TestContext.Create();
        var space = await TestDataFactory.SeedSpaceAsync(context.DbContext, context.SessionService.TenantId);
        var question =
            await TestDataFactory.SeedQuestionAsync(context.DbContext, context.SessionService.TenantId, space.Id);
        var answer = await TestDataFactory.SeedAnswerAsync(
            context.DbContext,
            context.SessionService.TenantId,
            question.Id,
            status: AnswerStatus.Active,
            visibility: VisibilityScope.Internal);

        await new AnswersUpdateAnswerCommandHandler(
            context.DbContext,
            context.SessionService,
            context.HttpContextAccessor).Handle(new AnswersUpdateAnswerCommand
        {
            Id = answer.Id,
            Request = new AnswerUpdateRequestDto
            {
                Headline = "Use the account security page",
                Body = answer.Body,
                Kind = answer.Kind,
                Status = answer.Status,
                Visibility = VisibilityScope.Internal,
                ContextNote = answer.ContextNote,
                AuthorLabel = answer.AuthorLabel,
                Sort = answer.Sort
            }
        }, CancellationToken.None);

        var updatedActivity = await context.DbContext.Activities
            .SingleAsync(activity => activity.AnswerId == answer.Id && activity.Kind == ActivityKind.AnswerUpdated);
        Assert.False(await context.DbContext.Activities
            .AnyAsync(activity => activity.AnswerId == answer.Id && activity.Kind == ActivityKind.AnswerArchived));
        Assert.Contains(context.SessionService.UserName!, updatedActivity.Notes);

        using var updatedMetadata = JsonDocument.Parse(updatedActivity.MetadataJson!);
        var updatedFields = updatedMetadata.RootElement
            .GetProperty("ChangedFields")
            .EnumerateArray()
            .Select(field => field.GetString())
            .ToList();
        Assert.Contains("Headline", updatedFields);
        Assert.DoesNotContain("Status", updatedFields);
        Assert.Equal(
            "Use the account security page",
            updatedMetadata.RootElement
                .GetProperty("Changes")
                .GetProperty("Headline")
                .GetProperty("After")
                .GetString());
    }

    [Fact]
    public async Task UpdateAnswer_UsesStatusActivityWhenStatusChanges()
    {
        using var context = TestContext.Create();
        var space = await TestDataFactory.SeedSpaceAsync(context.DbContext, context.SessionService.TenantId);
        var question =
            await TestDataFactory.SeedQuestionAsync(context.DbContext, context.SessionService.TenantId, space.Id);
        var answer = await TestDataFactory.SeedAnswerAsync(
            context.DbContext,
            context.SessionService.TenantId,
            question.Id,
            status: AnswerStatus.Active,
            visibility: VisibilityScope.Internal);

        await new AnswersUpdateAnswerCommandHandler(
            context.DbContext,
            context.SessionService,
            context.HttpContextAccessor).Handle(new AnswersUpdateAnswerCommand
        {
            Id = answer.Id,
            Request = new AnswerUpdateRequestDto
            {
                Headline = "Use the account security page",
                Body = answer.Body,
                Kind = answer.Kind,
                Status = AnswerStatus.Archived,
                Visibility = VisibilityScope.Internal,
                ContextNote = answer.ContextNote,
                AuthorLabel = answer.AuthorLabel,
                Sort = answer.Sort
            }
        }, CancellationToken.None);

        Assert.False(await context.DbContext.Activities
            .AnyAsync(activity => activity.AnswerId == answer.Id && activity.Kind == ActivityKind.AnswerUpdated));
        var archivedActivity = await context.DbContext.Activities
            .SingleAsync(activity => activity.AnswerId == answer.Id && activity.Kind == ActivityKind.AnswerArchived);
        Assert.Contains(context.SessionService.UserName!, archivedActivity.Notes);

        using var archivedMetadata = JsonDocument.Parse(archivedActivity.MetadataJson!);
        var archivedFields = archivedMetadata.RootElement
            .GetProperty("ChangedFields")
            .EnumerateArray()
            .Select(field => field.GetString())
            .ToList();
        Assert.Contains("Headline", archivedFields);
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
}
