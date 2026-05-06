using Querify.Models.QnA.Dtos.Tag;
using Querify.QnA.Portal.Business.Tag.Commands.CreateTag;
using Querify.QnA.Portal.Business.Tag.Queries.GetTag;
using Querify.QnA.Portal.Test.IntegrationTests.Helpers;
using Xunit;

namespace Querify.QnA.Portal.Test.IntegrationTests.Tests.Tag;

public class TagCommandQueryTests
{
    [Fact]
    public async Task CreateTag_PersistsEntityAndReturnsDto()
    {
        using var context = TestContext.Create();

        var createHandler = new TagsCreateTagCommandHandler(context.DbContext, context.SessionService);
        var id = await createHandler.Handle(new TagsCreateTagCommand
        {
            Request = new TagCreateRequestDto
            {
                Name = "billing"
            }
        }, CancellationToken.None);

        var getHandler = new TagsGetTagQueryHandler(context.DbContext, context.SessionService);
        var result = await getHandler.Handle(new TagsGetTagQuery { Id = id }, CancellationToken.None);

        Assert.Equal("billing", result.Name);
    }
}