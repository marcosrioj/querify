using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Models.QnA.Dtos.Answer;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Portal.Business.Answer.Commands.CreateAnswer;
using BaseFaq.QnA.Portal.Business.Answer.Queries.GetAnswer;
using BaseFaq.QnA.Portal.Test.IntegrationTests.Helpers;
using Xunit;

namespace BaseFaq.QnA.Portal.Test.IntegrationTests.Tests.Answer;

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
                    Visibility = VisibilityScope.Authenticated,
                    ContextNote = null,
                    Sort = 0
                }
            },
            CancellationToken.None));

        Assert.Equal((int)HttpStatusCode.UnprocessableEntity, exception.ErrorCode);
    }
}
