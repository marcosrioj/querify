using System.Net;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;

namespace Querify.Mcp.Common.Security;

public static class McpToolAuthorization
{
    public static void EnsureWriteToolsEnabled(bool enableWriteTools, string toolName)
    {
        if (enableWriteTools)
        {
            return;
        }

        throw new ApiErrorException(
            $"MCP write tool '{toolName}' is disabled. Enable McpServer:EnableWriteTools only for reviewed operator sessions.",
            (int)HttpStatusCode.Forbidden);
    }
}
