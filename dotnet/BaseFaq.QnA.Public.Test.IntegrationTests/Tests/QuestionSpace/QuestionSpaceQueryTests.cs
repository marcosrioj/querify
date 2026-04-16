using BaseFaq.QnA.Public.Business.QuestionSpace.Queries;
using BaseFaq.QnA.Public.Test.IntegrationTests.Helpers;
using Xunit;

namespace BaseFaq.QnA.Public.Test.IntegrationTests.Tests.QuestionSpace;

public class QuestionSpaceQueryTests
{
    [Fact]
    public async Task GetQuestionSpaceByKey_ReturnsPublicSpace()
    {
        using var context = TestContext.Create();
        var space = await TestDataFactory.SeedQuestionSpaceAsync(
            context.DbContext,
            context.TenantId,
            name: "Public Portal",
            key: "public-portal");
        var handler = new QuestionSpacesGetQuestionSpaceByKeyQueryHandler(
            context.DbContext,
            new TestSessionService(context.TenantId, context.UserId));

        var result = await handler.Handle(new QuestionSpacesGetQuestionSpaceByKeyQuery
        {
            Key = space.Key
        }, CancellationToken.None);

        Assert.Equal(space.Id, result.Id);
        Assert.Equal("Public Portal", result.Name);
        Assert.Equal("public-portal", result.Key);
    }
}
