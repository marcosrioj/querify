using Microsoft.Extensions.Options;
using ModelContextProtocol.Protocol;
using Querify.Common.EntityFramework.Tenant.Extensions;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Mcp.Common.Serialization;
using Querify.Mcp.Server.Infrastructure;
using Querify.Mcp.Server.Options;
using Querify.Mcp.Server.Prompts;
using Querify.Mcp.Server.Tools;
using Querify.QnA.Common.Domain.Options;
using Querify.QnA.Common.Persistence.QnADb.Extensions;
using Querify.QnA.Portal.Business.Answer.Extensions;
using Querify.QnA.Portal.Business.Question.Extensions;
using Querify.QnA.Portal.Business.Source.Extensions;
using Querify.QnA.Portal.Business.Space.Extensions;
using Querify.QnA.Public.Business.Search.Extensions;
using Querify.Tenant.Portal.Business.Billing.Extensions;
using Querify.Tenant.Portal.Business.Tenant.Extensions;
using Querify.Tenant.Portal.Business.User.Extensions;

namespace Querify.Mcp.Server.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddQuerifyMcpServer(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<McpServerOptions>()
            .BindConfiguration(McpServerOptions.SectionName);

        services
            .AddOptions<SourceUploadOptions>()
            .BindConfiguration(SourceUploadOptions.SectionName);

        services.AddHttpContextAccessor();
        services.AddTenantDb(configuration.GetConnectionString("TenantDb"));
        services.AddQnADb();

        services.AddScoped<McpSessionContext>();
        services.AddScoped<McpRequestContext>();
        services.AddScoped<ISessionService, McpSessionService>();

        services.AddAnswerBusiness();
        services.AddSourceBusiness();
        services.AddQuestionBusiness();
        services.AddSpaceBusiness();
        services.AddSearchBusiness();

        services.AddBillingBusiness();
        services.AddTenantBusiness();
        services.AddUserBusiness();

        services
            .AddMcpServer(options =>
            {
                options.ServerInfo = new Implementation
                {
                    Name = "querify-mcp-server",
                    Title = "Querify MCP Server",
                    Version = "0.1.0"
                };
                options.ServerInstructions =
                    "Use Querify MCP tools as adapters to module-owned CQRS behavior. Write tools create Draft/Internal QnA content and require explicit server enablement.";
                options.ScopeRequests = true;
            })
            .WithStdioServerTransport()
            .WithToolsFromAssembly(typeof(QnATools).Assembly, McpToolResultSerializer.JsonSerializerOptions)
            .WithPromptsFromAssembly(typeof(AgentPrompts).Assembly, McpToolResultSerializer.JsonSerializerOptions);

        return services;
    }
}
