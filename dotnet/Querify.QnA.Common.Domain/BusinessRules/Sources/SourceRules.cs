using System.Net;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Models.QnA.Enums;
using Querify.QnA.Common.Domain.Entities;

namespace Querify.QnA.Common.Domain.BusinessRules.Sources;

public static class SourceRules
{
    private static readonly IReadOnlyDictionary<string, string[]> ExtensionsByContentType =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["application/pdf"] = [".pdf"],
            ["image/png"] = [".png"],
            ["image/jpeg"] = [".jpg", ".jpeg"],
            ["video/mp4"] = [".mp4"],
            ["text/plain"] = [".txt"],
            ["text/markdown"] = [".md", ".markdown"]
        };

    public static void EnsureVisibilityAllowed(Source entity, VisibilityScope visibility)
    {
        if (visibility is not VisibilityScope.Public) return;

        if (entity.StorageKey is null && entity.LastVerifiedAtUtc is null)
            throw new ApiErrorException(
                "Sources must be verified before public exposure.",
                (int)HttpStatusCode.UnprocessableEntity);

        EnsurePublicVisibilityAllowedForUploadStatus(entity, visibility);
    }

    public static void EnsurePublicVisibilityAllowedForUploadStatus(Source source,
        VisibilityScope visibility)
    {
        if (visibility is not VisibilityScope.Public || source.StorageKey is null)
        {
            return;
        }

        if (source.UploadStatus is SourceUploadStatus.Verified)
        {
            return;
        }

        throw new ApiErrorException(
            "Uploaded sources must be verified before public exposure.",
            (int)HttpStatusCode.UnprocessableEntity);
    }

    public static void EnsureStorageKeyIsDownloadable(Source source)
    {
        EnsureStorageKeyIsDownloadable(source.StorageKey, source.UploadStatus);
    }

    public static void EnsureStorageKeyIsDownloadable(string? storageKey, SourceUploadStatus uploadStatus)
    {
        if (storageKey is null)
            throw new ApiErrorException(
                "Only uploaded sources can request a download URL.",
                (int)HttpStatusCode.UnprocessableEntity);

        if (uploadStatus is not SourceUploadStatus.Verified)
            throw new ApiErrorException(
                "Uploaded sources must be verified before download.",
                (int)HttpStatusCode.UnprocessableEntity);

        if (!SourceStorageKey.IsVerifiedKey(storageKey))
            throw new ApiErrorException(
                "Uploaded sources must use a verified storage key before download.",
                (int)HttpStatusCode.UnprocessableEntity);
    }

    public static bool IsUploadContentTypeAllowed(string? contentType,
        IEnumerable<string>? configuredAllowedContentTypes = null)
    {
        var normalizedContentType = NormalizeContentType(contentType);
        if (normalizedContentType is null)
        {
            return false;
        }

        var globalAllowlist = configuredAllowedContentTypes?.Select(NormalizeContentType)
            .Where(type => type is not null)
            .Select(type => type!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (globalAllowlist is { Count: > 0 } &&
            !globalAllowlist.Contains(normalizedContentType))
        {
            return false;
        }

        return ExtensionsByContentType.ContainsKey(normalizedContentType);
    }

    public static bool IsUploadFileNameAllowed(string fileName, string? contentType)
    {
        var normalizedContentType = NormalizeContentType(contentType);
        if (normalizedContentType is null ||
            !ExtensionsByContentType.TryGetValue(normalizedContentType, out var extensions))
        {
            return false;
        }

        var extension = Path.GetExtension(fileName.Replace('\\', '/')).ToLowerInvariant();
        return extensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
    }

    public static string? NormalizeContentType(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            return null;
        }

        return contentType.Split(';', 2)[0].Trim().ToLowerInvariant();
    }

    public static void EnsureReferenceSupportsPublicVisibility(
        VisibilityScope ownerVisibility,
        Source source,
        SourceRole role)
    {
        if (ownerVisibility is not VisibilityScope.Public) return;

        if (role is SourceRole.Reference &&
            source.Visibility is not VisibilityScope.Public)
            throw new ApiErrorException(
                "Public references require a publicly visible source.",
                (int)HttpStatusCode.UnprocessableEntity);
    }
}
