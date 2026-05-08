using Amazon.Runtime;
using Amazon.S3;
using Microsoft.Extensions.Options;
using Querify.Common.Infrastructure.Storage.Options;
using Querify.Common.Infrastructure.Storage.Services;
using Xunit;

namespace Querify.QnA.Portal.Test.IntegrationTests.Tests.Storage;

public sealed class S3ObjectStoragePresignTests
{
    [Fact]
    public async Task PresignPutAsync_HttpPublicEndpoint_GeneratesHttpUrl()
    {
        var options = CreateOptions();
        using var internalClient = CreateClient(options.Endpoint, options);
        using var storage = new S3ObjectStorage(internalClient, Options.Create(options));

        var result = await storage.PresignPutAsync(
            "tenant/sources/source/staging/test.pdf",
            "application/pdf",
            expectedSizeBytes: 1,
            CancellationToken.None);

        Assert.Equal(Uri.UriSchemeHttp, result.Url.Scheme);
        Assert.Equal("localhost", result.Url.Host);
        Assert.Equal(5900, result.Url.Port);
    }

    [Fact]
    public async Task PresignGetAsync_HttpPublicEndpoint_GeneratesHttpUrl()
    {
        var options = CreateOptions();
        using var internalClient = CreateClient(options.Endpoint, options);
        using var storage = new S3ObjectStorage(internalClient, Options.Create(options));

        var result = await storage.PresignGetAsync(
            "tenant/sources/source/verified/test.pdf",
            TimeSpan.FromMinutes(5),
            CancellationToken.None);

        Assert.Equal(Uri.UriSchemeHttp, result.Scheme);
        Assert.Equal("localhost", result.Host);
        Assert.Equal(5900, result.Port);
    }

    private static ObjectStorageOptions CreateOptions()
    {
        return new ObjectStorageOptions
        {
            Endpoint = "http://minio:9000",
            PublicEndpoint = "http://localhost:5900",
            Region = "us-east-1",
            AccessKey = "minio",
            SecretKey = "Pass123$",
            Bucket = "querify-sources",
            ForcePathStyle = true
        };
    }

    private static AmazonS3Client CreateClient(string endpoint, ObjectStorageOptions options)
    {
        return new AmazonS3Client(
            new BasicAWSCredentials(options.AccessKey, options.SecretKey),
            new AmazonS3Config
            {
                ServiceURL = endpoint,
                AuthenticationRegion = options.Region,
                ForcePathStyle = options.ForcePathStyle,
                UseHttp = true
            });
    }
}
