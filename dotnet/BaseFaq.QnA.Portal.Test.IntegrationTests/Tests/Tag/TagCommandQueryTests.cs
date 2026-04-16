using BaseFaq.Models.QnA.Dtos.Tag;
using BaseFaq.QnA.Portal.Business.Tag.Commands.CreateTag;
using BaseFaq.QnA.Portal.Business.Tag.Queries.GetTag;
using BaseFaq.QnA.Portal.Test.IntegrationTests.Helpers;
using Xunit;

namespace BaseFaq.QnA.Portal.Test.IntegrationTests.Tests.Tag;

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
