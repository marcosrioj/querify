using System.Text.RegularExpressions;

namespace Querify.QnA.Common.Domain.BusinessRules.Sources;

public static partial class SourceStorageKey
{
    private const string SourcesSegment = "sources";
    private const string StagingSegment = "staging";
    private const string VerifiedSegment = "verified";
    private const string QuarantineSegment = "quarantine";
    private const string FallbackFileName = "upload";

    public static string BuildStagingKey(Guid tenantId, Guid sourceId, string fileName)
    {
        return BuildKey(tenantId, sourceId, StagingSegment, fileName);
    }

    public static string BuildVerifiedKey(Guid tenantId, Guid sourceId, string fileName)
    {
        return BuildKey(tenantId, sourceId, VerifiedSegment, fileName);
    }

    public static string ToVerifiedKey(string stagingKey)
    {
        return ConvertStagingKey(stagingKey, VerifiedSegment);
    }

    public static string ToQuarantineKey(string stagingKey)
    {
        return ConvertStagingKey(stagingKey, QuarantineSegment);
    }

    public static string ToLocator(string storageKey)
    {
        var segments = ParseKey(storageKey);
        return $"{ToLocatorStage(segments.Stage)}/{segments.FileName}";
    }

    public static bool IsStagingKey(string? key)
    {
        return IsKeyInStage(key, StagingSegment);
    }

    public static bool IsVerifiedKey(string? key)
    {
        return IsKeyInStage(key, VerifiedSegment);
    }

    private static string BuildKey(Guid tenantId, Guid sourceId, string stage, string fileName)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("Tenant id is required.", nameof(tenantId));
        }

        if (sourceId == Guid.Empty)
        {
            throw new ArgumentException("Source id is required.", nameof(sourceId));
        }

        return $"{tenantId}/sources/{sourceId}/{stage}/{SanitizeFileName(fileName)}";
    }

    private static string ConvertStagingKey(string stagingKey, string targetStage)
    {
        var segments = ParseKey(stagingKey);
        if (!StringComparer.Ordinal.Equals(segments.Stage, StagingSegment))
        {
            throw new ArgumentException("Storage key must be a staging source key.", nameof(stagingKey));
        }

        return $"{segments.TenantId}/sources/{segments.SourceId}/{targetStage}/{segments.FileName}";
    }

    private static string ToLocatorStage(string stage)
    {
        return stage switch
        {
            StagingSegment => "Staging",
            VerifiedSegment => "Verified",
            QuarantineSegment => "Quarantine",
            _ => $"{char.ToUpperInvariant(stage[0])}{stage[1..]}"
        };
    }

    private static bool IsKeyInStage(string? key, string stage)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        try
        {
            var segments = ParseKey(key);
            return StringComparer.Ordinal.Equals(segments.Stage, stage);
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    private static SourceKeySegments ParseKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Storage key is required.", nameof(key));
        }

        var segments = key.Split('/', StringSplitOptions.None);
        if (segments.Length != 5 ||
            !Guid.TryParse(segments[0], out var tenantId) ||
            !StringComparer.Ordinal.Equals(segments[1], SourcesSegment) ||
            !Guid.TryParse(segments[2], out var sourceId) ||
            string.IsNullOrWhiteSpace(segments[3]) ||
            string.IsNullOrWhiteSpace(segments[4]))
        {
            throw new ArgumentException("Storage key is not a valid source object key.", nameof(key));
        }

        return new SourceKeySegments(tenantId, sourceId, segments[3], segments[4]);
    }

    private static string SanitizeFileName(string fileName)
    {
        var strippedFileName = StripPath(fileName);
        var sanitized = UnsafeFileNameCharacters().Replace(strippedFileName, "-");

        return string.IsNullOrWhiteSpace(sanitized) || sanitized.All(character => character == '.')
            ? FallbackFileName
            : sanitized;
    }

    private static string StripPath(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return FallbackFileName;
        }

        var normalized = fileName.Replace('\\', '/');
        return normalized
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .LastOrDefault()
            ?.Trim() ?? FallbackFileName;
    }

    [GeneratedRegex("[^a-zA-Z0-9._-]", RegexOptions.CultureInvariant)]
    private static partial Regex UnsafeFileNameCharacters();

    private sealed record SourceKeySegments(Guid TenantId, Guid SourceId, string Stage, string FileName);
}
