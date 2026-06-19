using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Mcp.Common.Security;
using Xunit;

namespace Querify.Mcp.Server.Test.IntegrationTests.Security;

public sealed class McpToolAuthorizationTests
{
    [Fact]
    public void EnsureWriteToolsEnabledRejectsDisabledWrites()
    {
        Assert.Throws<ApiErrorException>(() =>
            McpToolAuthorization.EnsureWriteToolsEnabled(false, "qna_create_question"));
    }

    [Fact]
    public void EnsureWriteToolsEnabledAllowsEnabledWrites()
    {
        McpToolAuthorization.EnsureWriteToolsEnabled(true, "qna_create_question");
    }
}
