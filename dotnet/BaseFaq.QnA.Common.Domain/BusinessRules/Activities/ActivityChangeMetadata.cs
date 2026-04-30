using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace BaseFaq.QnA.Common.Domain.BusinessRules.Activities;

public static class ActivityChangeMetadata
{
    private const int DefaultMaxLength = 4000;
    private const int PrimaryStringPreviewLength = 512;
    private const int CompactStringPreviewLength = 128;

    public static string? Create(
        string entity,
        string operation,
        Guid entityId,
        IReadOnlyDictionary<string, object?> before,
        IReadOnlyDictionary<string, object?> after,
        IReadOnlyDictionary<string, object?>? context = null,
        IEnumerable<string>? ignoredFields = null,
        int maxLength = DefaultMaxLength)
    {
        var ignored = ignoredFields is null
            ? new HashSet<string>(StringComparer.Ordinal)
            : new HashSet<string>(ignoredFields, StringComparer.Ordinal);
        var changedFields = GetChangedFields(before, after, ignored);

        if (changedFields.Count == 0)
            return null;

        var serialized = Serialize(BuildPayload(
            entity,
            operation,
            entityId,
            before,
            after,
            context,
            changedFields,
            PrimaryStringPreviewLength,
            false));

        if (serialized.Length <= maxLength)
            return serialized;

        serialized = Serialize(BuildPayload(
            entity,
            operation,
            entityId,
            before,
            after,
            context,
            changedFields,
            CompactStringPreviewLength,
            true));

        if (serialized.Length <= maxLength)
            return serialized;

        return Serialize(new ActivityChangeMetadataPayload(
            entity,
            operation,
            entityId,
            NormalizeContext(context, CompactStringPreviewLength),
            changedFields,
            new Dictionary<string, ActivityFieldChange>(StringComparer.Ordinal),
            true));
    }

    private static List<string> GetChangedFields(
        IReadOnlyDictionary<string, object?> before,
        IReadOnlyDictionary<string, object?> after,
        HashSet<string> ignored)
    {
        return before.Keys
            .Concat(after.Keys)
            .Where(field => !ignored.Contains(field))
            .Distinct(StringComparer.Ordinal)
            .Where(field =>
            {
                before.TryGetValue(field, out var beforeValue);
                after.TryGetValue(field, out var afterValue);
                return !ValuesEqual(beforeValue, afterValue);
            })
            .OrderBy(field => field, StringComparer.Ordinal)
            .ToList();
    }

    private static ActivityChangeMetadataPayload BuildPayload(
        string entity,
        string operation,
        Guid entityId,
        IReadOnlyDictionary<string, object?> before,
        IReadOnlyDictionary<string, object?> after,
        IReadOnlyDictionary<string, object?>? context,
        IReadOnlyList<string> changedFields,
        int stringPreviewLength,
        bool compacted)
    {
        var changes = changedFields.ToDictionary(
            field => field,
            field =>
            {
                before.TryGetValue(field, out var beforeValue);
                after.TryGetValue(field, out var afterValue);

                return new ActivityFieldChange(
                    NormalizeValue(beforeValue, stringPreviewLength),
                    NormalizeValue(afterValue, stringPreviewLength));
            },
            StringComparer.Ordinal);

        return new ActivityChangeMetadataPayload(
            entity,
            operation,
            entityId,
            NormalizeContext(context, stringPreviewLength),
            changedFields,
            changes,
            compacted);
    }

    private static Dictionary<string, object?>? NormalizeContext(
        IReadOnlyDictionary<string, object?>? context,
        int stringPreviewLength)
    {
        return context?.ToDictionary(
            item => item.Key,
            item => NormalizeValue(item.Value, stringPreviewLength),
            StringComparer.Ordinal);
    }

    private static object? NormalizeValue(object? value, int stringPreviewLength)
    {
        if (value is not string text)
            return value;

        if (text.Length <= stringPreviewLength)
            return text;

        return new ActivityTextPreview(
            text[..stringPreviewLength],
            text.Length,
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant());
    }

    private static bool ValuesEqual(object? before, object? after)
    {
        if (before is null || after is null)
            return before is null && after is null;

        return before.Equals(after);
    }

    private static string Serialize(object value)
    {
        return JsonSerializer.Serialize(value);
    }

    private sealed record ActivityChangeMetadataPayload(
        string Entity,
        string Operation,
        Guid EntityId,
        IReadOnlyDictionary<string, object?>? Context,
        IReadOnlyList<string> ChangedFields,
        IReadOnlyDictionary<string, ActivityFieldChange> Changes,
        bool Compacted);

    private sealed record ActivityFieldChange(object? Before, object? After);

    private sealed record ActivityTextPreview(string Preview, int Length, string Sha256);
}
