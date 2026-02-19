using System.Text.Json;

namespace BaseFaq.AI.Business.Common.Utilities;

public static class JsonPayloadReader
{
    public static string ExtractJsonObject(string content)
    {
        var raw = UnwrapMarkdownJsonFence(content).Trim();

        try
        {
            using var parsed = JsonDocument.Parse(raw);
            return raw;
        }
        catch (JsonException)
        {
            var firstBrace = raw.IndexOf('{');
            var lastBrace = raw.LastIndexOf('}');

            if (firstBrace >= 0 && lastBrace > firstBrace)
            {
                var candidate = raw[firstBrace..(lastBrace + 1)];
                using var parsed = JsonDocument.Parse(candidate);
                return candidate;
            }

            throw;
        }
    }

    private static string UnwrapMarkdownJsonFence(string content)
    {
        var raw = content.Trim();

        if (!raw.StartsWith("```", StringComparison.Ordinal))
        {
            return raw;
        }

        var lines = raw
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Where(line => !line.TrimStart().StartsWith("```", StringComparison.Ordinal))
            .ToArray();

        return string.Join(Environment.NewLine, lines);
    }
}