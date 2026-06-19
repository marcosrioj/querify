namespace Querify.Mcp.Server.Options;

public sealed class McpServerOptions
{
    public const string SectionName = "McpServer";

    public Guid? DefaultTenantId { get; set; }

    public Guid ServiceUserId { get; set; }

    public string? ServiceUserName { get; set; } = "system:mcp";

    public bool EnableWriteTools { get; set; }

    public int ToolResultMaxItems { get; set; } = 20;

    public bool IncludeInternalIds { get; set; } = true;
}
