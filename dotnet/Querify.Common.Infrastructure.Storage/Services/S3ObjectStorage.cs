using System.Net;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Querify.Common.Infrastructure.Storage.Abstractions;
using Querify.Common.Infrastructure.Storage.Options;
using Microsoft.Extensions.Options;

namespace Querify.Common.Infrastructure.Storage.Services;

public sealed class S3ObjectStorage : IObjectStorage, IDisposable
{
    private const string ContentTypeHeader = "Content-Type";
    private const string ServerSideEncryptionHeader = "x-amz-server-side-encryption";

    private readonly IAmazonS3 _client;
    private readonly ObjectStorageOptions _options;
    private readonly IAmazonS3 _presignClient;
    private readonly bool _disposePresignClient;

    public S3ObjectStorage(IAmazonS3 client, IOptions<ObjectStorageOptions> options)
    {
        _client = client;
        _options = options.Value;

        var publicEndpoint = string.IsNullOrWhiteSpace(_options.PublicEndpoint)
            ? _options.Endpoint
            : _options.PublicEndpoint;

        if (StringComparer.OrdinalIgnoreCase.Equals(publicEndpoint, _options.Endpoint))
        {
            _presignClient = client;
            return;
        }

        var credentials = new BasicAWSCredentials(_options.AccessKey, _options.SecretKey);
        _presignClient = new AmazonS3Client(
            credentials,
            CreateConfig(publicEndpoint, _options.Region, _options.ForcePathStyle));
        _disposePresignClient = true;
    }

    public Task<PresignedPutResult> PresignPutAsync(
        string key,
        string contentType,
        long expectedSizeBytes,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(expectedSizeBytes);

        var expiresAtUtc = DateTime.UtcNow.AddMinutes(_options.UploadPresignTtlMinutes);
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _options.Bucket,
            Key = key,
            Verb = HttpVerb.PUT,
            ContentType = contentType,
            Expires = expiresAtUtc
        };

        ApplyServerSideEncryption(request);

        var requiredHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [ContentTypeHeader] = contentType
        };

        if (!string.IsNullOrWhiteSpace(_options.ServerSideEncryptionMode))
        {
            requiredHeaders[ServerSideEncryptionHeader] = _options.ServerSideEncryptionMode;
        }

        var url = _presignClient.GetPreSignedURL(request);
        return Task.FromResult(new PresignedPutResult(new Uri(url), requiredHeaders, expiresAtUtc));
    }

    public Task<Uri> PresignGetAsync(string key, TimeSpan ttl, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var request = new GetPreSignedUrlRequest
        {
            BucketName = _options.Bucket,
            Key = key,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.Add(ttl)
        };

        var url = _presignClient.GetPreSignedURL(request);
        return Task.FromResult(new Uri(url));
    }

    public async Task<ObjectMetadata?> HeadAsync(string key, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        try
        {
            var response = await _client.GetObjectMetadataAsync(new GetObjectMetadataRequest
            {
                BucketName = _options.Bucket,
                Key = key
            }, ct);

            return new ObjectMetadata(
                response.ContentLength,
                response.Headers.ContentType ?? string.Empty,
                response.ETag ?? string.Empty,
                response.LastModified?.ToUniversalTime() ?? DateTime.MinValue);
        }
        catch (AmazonS3Exception exception) when (exception.StatusCode is HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public Task CopyAsync(string sourceKey, string destinationKey, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationKey);

        var request = new CopyObjectRequest
        {
            SourceBucket = _options.Bucket,
            SourceKey = sourceKey,
            DestinationBucket = _options.Bucket,
            DestinationKey = destinationKey
        };

        ApplyServerSideEncryption(request);
        return _client.CopyObjectAsync(request, ct);
    }

    public Task DeleteAsync(string key, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        return _client.DeleteObjectAsync(new DeleteObjectRequest
        {
            BucketName = _options.Bucket,
            Key = key
        }, ct);
    }

    public async Task<Stream> OpenReadAsync(string key, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var response = await _client.GetObjectAsync(new GetObjectRequest
        {
            BucketName = _options.Bucket,
            Key = key
        }, ct);

        return new S3ObjectResponseStream(response);
    }

    public void Dispose()
    {
        if (_disposePresignClient)
        {
            (_presignClient as IDisposable)?.Dispose();
        }
    }

    private void ApplyServerSideEncryption(GetPreSignedUrlRequest request)
    {
        var method = ResolveServerSideEncryptionMethod();
        if (method is not null)
        {
            request.ServerSideEncryptionMethod = method;
        }
    }

    private void ApplyServerSideEncryption(CopyObjectRequest request)
    {
        var method = ResolveServerSideEncryptionMethod();
        if (method is not null)
        {
            request.ServerSideEncryptionMethod = method;
        }
    }

    private ServerSideEncryptionMethod? ResolveServerSideEncryptionMethod()
    {
        return string.IsNullOrWhiteSpace(_options.ServerSideEncryptionMode)
            ? null
            : ServerSideEncryptionMethod.FindValue(_options.ServerSideEncryptionMode);
    }

    private static AmazonS3Config CreateConfig(string endpoint, string region, bool forcePathStyle)
    {
        return new AmazonS3Config
        {
            ServiceURL = endpoint,
            AuthenticationRegion = region,
            ForcePathStyle = forcePathStyle
        };
    }

    private sealed class S3ObjectResponseStream(GetObjectResponse response) : Stream
    {
        private readonly Stream _inner = response.ResponseStream;

        public override bool CanRead => _inner.CanRead;
        public override bool CanSeek => _inner.CanSeek;
        public override bool CanWrite => false;
        public override long Length => _inner.Length;

        public override long Position
        {
            get => _inner.Position;
            set => _inner.Position = value;
        }

        public override void Flush()
        {
            _inner.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _inner.Read(buffer, offset, count);
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return _inner.ReadAsync(buffer, cancellationToken);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _inner.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            return _inner.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _inner.Dispose();
                response.Dispose();
            }

            base.Dispose(disposing);
        }

        public override async ValueTask DisposeAsync()
        {
            await _inner.DisposeAsync();
            response.Dispose();
            await base.DisposeAsync();
        }
    }
}
