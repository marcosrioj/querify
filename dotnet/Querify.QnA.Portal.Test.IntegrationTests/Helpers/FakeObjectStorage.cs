using Querify.Common.Infrastructure.Storage.Abstractions;

namespace Querify.QnA.Portal.Test.IntegrationTests.Helpers;

public sealed class FakeObjectStorage : IObjectStorage
{
    private readonly Dictionary<string, FakeObject> _objects = new(StringComparer.Ordinal);

    public List<string> DeletedKeys { get; } = [];

    public void Put(string key, byte[] content, string contentType)
    {
        _objects[key] = new FakeObject(content, contentType);
    }

    public Task<PresignedPutResult> PresignPutAsync(
        string key,
        string contentType,
        long expectedSizeBytes,
        CancellationToken ct)
    {
        return Task.FromResult(new PresignedPutResult(
            new Uri($"https://storage.example.test/{Uri.EscapeDataString(key)}"),
            new Dictionary<string, string>
            {
                ["Content-Type"] = contentType
            },
            DateTime.UtcNow.AddMinutes(10)));
    }

    public Task<Uri> PresignGetAsync(string key, TimeSpan ttl, CancellationToken ct)
    {
        return Task.FromResult(new Uri($"https://storage.example.test/download/{Uri.EscapeDataString(key)}"));
    }

    public Task<ObjectMetadata?> HeadAsync(string key, CancellationToken ct)
    {
        if (!_objects.TryGetValue(key, out var value))
        {
            return Task.FromResult<ObjectMetadata?>(null);
        }

        return Task.FromResult<ObjectMetadata?>(new ObjectMetadata(
            value.Content.LongLength,
            value.ContentType,
            "etag",
            DateTime.UtcNow));
    }

    public Task CopyAsync(string sourceKey, string destinationKey, CancellationToken ct)
    {
        if (_objects.TryGetValue(sourceKey, out var value))
        {
            _objects[destinationKey] = value;
        }

        return Task.CompletedTask;
    }

    public Task DeleteAsync(string key, CancellationToken ct)
    {
        DeletedKeys.Add(key);
        _objects.Remove(key);
        return Task.CompletedTask;
    }

    public Task<Stream> OpenReadAsync(string key, CancellationToken ct)
    {
        if (!_objects.TryGetValue(key, out var value))
        {
            throw new FileNotFoundException("Object not found.", key);
        }

        return Task.FromResult<Stream>(new MemoryStream(value.Content, writable: false));
    }

    private sealed record FakeObject(byte[] Content, string ContentType);
}
