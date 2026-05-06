using Querify.QnA.Public.Business.Space.Queries.GetSpaceBySlug;
using Querify.QnA.Public.Test.IntegrationTests.Helpers;
using Xunit;

namespace Querify.QnA.Public.Test.IntegrationTests.Tests.Space;

public class SpaceQueryTests
{
    [Fact]
    public async Task GetSpaceBySlug_ReturnsPublicSpace()
    {
        using var context = TestContext.Create();
        var space = await TestDataFactory.SeedSpaceAsync(
            context.DbContext,
            context.TenantId,
            "Public Portal",
            "public-portal");
        var handler = new SpacesGetSpaceBySlugQueryHandler(
            context.DbContext,
            new TestClientKeyContextService(context.ClientKey),
            new TestTenantClientKeyResolver(context.TenantId, context.ClientKey),
            context.HttpContextAccessor);

        var result = await handler.Handle(new SpacesGetSpaceBySlugQuery
        {
            Slug = space.Slug
        }, CancellationToken.None);

        Assert.Equal(space.Id, result.Id);
        Assert.Equal("Public Portal", result.Name);
        Assert.Equal("public-portal", result.Slug);
    }
}
