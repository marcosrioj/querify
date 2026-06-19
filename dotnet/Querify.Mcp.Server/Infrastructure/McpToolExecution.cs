using ModelContextProtocol;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Mcp.Common.Serialization;

namespace Querify.Mcp.Server.Infrastructure;

public static class McpToolExecution
{
    public static async Task<string> ExecuteAsync(Func<Task<object?>> action)
    {
        try
        {
            var result = await action();
            return McpToolResultSerializer.Serialize(result);
        }
        catch (ApiErrorException exception)
        {
            throw new McpException(exception.Message, exception);
        }
        catch (InvalidOperationException exception)
        {
            throw new McpException(exception.Message, exception);
        }
    }
}
