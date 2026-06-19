using System.Reflection;
using ModelContextProtocol.Server;
using Querify.Mcp.Common.Constants;
using Querify.Mcp.Server.Prompts;
using Querify.Mcp.Server.Tools;
using Xunit;

namespace Querify.Mcp.Server.Test.IntegrationTests.Tools;

public sealed class McpToolMetadataTests
{
    [Fact]
    public void QnAToolsExposeExpectedStageTwoToolNames()
    {
        var toolNames = GetToolNames(typeof(QnATools));

        Assert.Equal(
            [
                McpToolNames.QnAActivateAnswer,
                McpToolNames.QnACreateAnswer,
                McpToolNames.QnACreateQuestion,
                McpToolNames.QnACreateSource,
                McpToolNames.QnAGetQuestion,
                McpToolNames.QnAGetSource,
                McpToolNames.QnAGetSpace,
                McpToolNames.QnALinkAnswerSource,
                McpToolNames.QnALinkQuestionSource,
                McpToolNames.QnAListQuestions,
                McpToolNames.QnAListSources,
                McpToolNames.QnAListSpaces,
                McpToolNames.QnASearch
            ],
            toolNames);
    }

    [Fact]
    public void TenantToolsExposeReadOnlyStageOneToolNames()
    {
        var toolNames = GetToolNames(typeof(TenantTools));

        Assert.Equal(
            [
                McpToolNames.TenantGetBillingSummary,
                McpToolNames.TenantGetClientKey,
                McpToolNames.TenantGetProfile,
                McpToolNames.TenantGetSubscription,
                McpToolNames.TenantListMembers,
                McpToolNames.TenantListWorkspaces
            ],
            toolNames);
    }

    [Fact]
    public void AgentPromptsExposeExpectedStageOnePromptNames()
    {
        var promptNames = typeof(AgentPrompts)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Select(method => method.GetCustomAttribute<McpServerPromptAttribute>())
            .Where(attribute => attribute is not null)
            .Select(attribute => attribute!.Name)
            .Order(StringComparer.Ordinal)
            .ToList();

        Assert.Equal(
            [
                McpPromptNames.QnAAssistant,
                McpPromptNames.TenantAssistant
            ],
            promptNames);
    }

    [Fact]
    public void TenantToolsAreReadOnly()
    {
        var writeTools = typeof(TenantTools)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Select(method => method.GetCustomAttribute<McpServerToolAttribute>())
            .Where(attribute => attribute is not null && !attribute.ReadOnly)
            .Select(attribute => attribute!.Name)
            .ToList();

        Assert.Empty(writeTools);
    }

    private static List<string?> GetToolNames(Type type)
    {
        return type
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Select(method => method.GetCustomAttribute<McpServerToolAttribute>())
            .Where(attribute => attribute is not null)
            .Select(attribute => attribute!.Name)
            .Order(StringComparer.Ordinal)
            .ToList();
    }
}
