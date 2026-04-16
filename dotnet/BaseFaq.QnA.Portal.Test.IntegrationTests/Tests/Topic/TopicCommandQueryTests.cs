using BaseFaq.Models.QnA.Dtos.Topic;
using BaseFaq.QnA.Portal.Business.Topic.Commands.CreateTopic;
using BaseFaq.QnA.Portal.Business.Topic.Commands.DeleteTopic;
using BaseFaq.QnA.Portal.Business.Topic.Commands.UpdateTopic;
using BaseFaq.QnA.Portal.Business.Topic.Queries.GetTopic;
using BaseFaq.QnA.Portal.Business.Topic.Queries.GetTopicList;
using BaseFaq.QnA.Portal.Test.IntegrationTests.Helpers;
using Xunit;

namespace BaseFaq.QnA.Portal.Test.IntegrationTests.Tests.Topic;

public class TopicCommandQueryTests
{
    [Fact]
    public async Task CreateTopic_PersistsEntityAndReturnsDto()
    {
        using var context = TestContext.Create();

        var createHandler = new TopicsCreateTopicCommandHandler(context.DbContext, context.SessionService);
        var id = await createHandler.Handle(new TopicsCreateTopicCommand
        {
            Request = new TopicCreateRequestDto
            {
                Name = "billing",
                Category = "product",
                Description = "Billing related questions."
            }
        }, CancellationToken.None);

        var getHandler = new TopicsGetTopicQueryHandler(context.DbContext, context.SessionService);
        var result = await getHandler.Handle(new TopicsGetTopicQuery { Id = id }, CancellationToken.None);

        Assert.Equal("billing", result.Name);
        Assert.Equal("product", result.Category);
        Assert.Equal("Billing related questions.", result.Description);
    }
}
