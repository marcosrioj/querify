using System.Text.Json;
using System.Text.Json.Serialization;

namespace Querify.Mcp.Common.Serialization;

public static class McpToolResultSerializer
{
    public static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    static McpToolResultSerializer()
    {
        JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public static string Serialize(object? value)
    {
        return JsonSerializer.Serialize(value, JsonSerializerOptions);
    }
}
