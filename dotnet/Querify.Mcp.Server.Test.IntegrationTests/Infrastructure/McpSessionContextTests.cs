using Querify.Mcp.Server.Infrastructure;
using Xunit;

namespace Querify.Mcp.Server.Test.IntegrationTests.Infrastructure;

public sealed class McpSessionContextTests
{
    [Fact]
    public void SetRejectsEmptyTenantId()
    {
        var context = new McpSessionContext();

        Assert.ThrowsAny<Exception>(() =>
            context.Set(Guid.Empty, Guid.NewGuid(), "system:mcp"));
    }

    [Fact]
    public void SetRejectsEmptyUserId()
    {
        var context = new McpSessionContext();

        Assert.Throws<InvalidOperationException>(() =>
            context.Set(Guid.NewGuid(), Guid.Empty, "system:mcp"));
    }

    [Fact]
    public void SetStoresTenantAndUserContext()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var context = new McpSessionContext();

        context.Set(tenantId, userId, "system:mcp");

        Assert.True(context.IsConfigured);
        Assert.Equal(tenantId, context.TenantId);
        Assert.Equal(userId, context.UserId);
        Assert.Equal("system:mcp", context.UserName);
    }
}
