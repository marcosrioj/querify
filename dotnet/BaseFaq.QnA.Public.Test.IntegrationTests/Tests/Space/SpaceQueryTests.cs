using BaseFaq.QnA.Public.Business.Space.Queries.GetSpaceByKey;
using BaseFaq.QnA.Public.Test.IntegrationTests.Helpers;
using Xunit;

namespace BaseFaq.QnA.Public.Test.IntegrationTests.Tests.Space;

public class SpaceQueryTests
{
    [Fact]
    public async Task GetSpaceByKey_ReturnsPublicSpace()
    {
        using var context = TestContext.Create();
        var space = await TestDataFactory.SeedSpaceAsync(
            context.DbContext,
            context.TenantId,
            "Public Portal",
            "public-portal");
        var handler = new SpacesGetSpaceByKeyQueryHandler(
            context.DbContext,
            new TestClientKeyContextService(context.ClientKey),
            new TestTenantClientKeyResolver(context.TenantId, context.ClientKey),
            context.HttpContextAccessor);

        var result = await handler.Handle(new SpacesGetSpaceByKeyQuery
        {
            Key = space.Key
        }, CancellationToken.None);

        Assert.Equal(space.Id, result.Id);
        Assert.Equal("Public Portal", result.Name);
        Assert.Equal("public-portal", result.Key);
    }
}