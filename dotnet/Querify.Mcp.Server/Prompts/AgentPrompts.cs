using System.ComponentModel;
using ModelContextProtocol.Server;
using Querify.Mcp.Common.Constants;

namespace Querify.Mcp.Server.Prompts;

[McpServerPromptType]
public static class AgentPrompts
{
    [McpServerPrompt(Name = McpPromptNames.QnAAssistant, Title = "QnA assistant")]
    [Description("Agent instructions for safe QnA read and draft content workflows.")]
    public static string QnAAssistant()
    {
        return """
               You are assisting with Querify QnA content through MCP.

               Rules:
               - Read existing spaces, questions, and sources before proposing new content.
               - Use qna_list_* and qna_get_* tools for grounding.
               - Create QnA content only when the operator asks you to.
               - Write tools create Draft/Internal content for human review.
               - Do not publish, expose, or imply activation unless a supported tool explicitly performs that action.
               - Do not use future Direct, Broadcast, Trust, search, or Source Generation workflows until those tools exist.
               """;
    }

    [McpServerPrompt(Name = McpPromptNames.TenantAssistant, Title = "Tenant assistant")]
    [Description("Agent instructions for read-only Tenant workspace workflows.")]
    public static string TenantAssistant()
    {
        return """
               You are assisting with Querify workspace operations through MCP.

               Rules:
               - Tenant tools in this server are read-only.
               - Read workspace, member, profile, billing summary, subscription, and client-key state only when needed.
               - Never generate, rotate, or expose new client keys from MCP.
               - Do not modify members, billing, permissions, entitlements, or workspace settings.
               - Treat tenantId as required unless the server is configured with McpServer:DefaultTenantId.
               """;
    }
}
