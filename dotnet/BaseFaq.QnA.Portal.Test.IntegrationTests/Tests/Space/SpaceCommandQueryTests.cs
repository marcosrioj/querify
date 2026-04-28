using BaseFaq.Models.QnA.Dtos.Space;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Portal.Business.Space.Commands.CreateSpace;
using BaseFaq.QnA.Portal.Business.Space.Queries.GetSpace;
using BaseFaq.QnA.Portal.Test.IntegrationTests.Helpers;
using Xunit;

namespace BaseFaq.QnA.Portal.Test.IntegrationTests.Tests.Space;

public class SpaceCommandQueryTests
{
    [Fact]
    public async Task CreateSpace_PersistsOperatingModelAndExposure()
    {
        using var context = TestContext.Create();
        var createHandler =
            new SpacesCreateSpaceCommandHandler(context.DbContext, context.SessionService);

        var id = await createHandler.Handle(new SpacesCreateSpaceCommand
        {
            Request = new SpaceCreateRequestDto
            {
                Name = "Portal Support",
                Slug = "portal-support",
                Language = "en-US",
                Summary = "Support questions for portal users.",
                Status = SpaceStatus.Active,
                Visibility = VisibilityScope.Public,
                AcceptsQuestions = true,
                AcceptsAnswers = true
            }
        }, CancellationToken.None);

        var getHandler = new SpacesGetSpaceQueryHandler(context.DbContext, context.SessionService);
        var result =
            await getHandler.Handle(new SpacesGetSpaceQuery { Id = id }, CancellationToken.None);

        Assert.Equal("Portal Support", result.Name);
        Assert.Equal("portal-support", result.Slug);
        Assert.Equal(VisibilityScope.Public, result.Visibility);
        Assert.Equal("en-US", result.Language);
        Assert.Equal(SpaceStatus.Active, result.Status);
        Assert.True(result.AcceptsQuestions);
    }
}
