using Amazon.Runtime;
using Amazon.S3;
using Querify.Common.Infrastructure.Storage.Abstractions;
using Querify.Common.Infrastructure.Storage.Options;
using Querify.Common.Infrastructure.Storage.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Querify.Common.Infrastructure.Storage.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddObjectStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ObjectStorageOptions>()
            .BindConfiguration(ObjectStorageOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IAmazonS3>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<ObjectStorageOptions>>().Value;
            var credentials = new BasicAWSCredentials(options.AccessKey, options.SecretKey);
            return new AmazonS3Client(credentials, CreateConfig(options.Endpoint, options.Region, options.ForcePathStyle));
        });

        services.AddSingleton<IObjectStorage, S3ObjectStorage>();
        return services;
    }

    private static AmazonS3Config CreateConfig(string endpoint, string region, bool forcePathStyle)
    {
        return new AmazonS3Config
        {
            ServiceURL = endpoint,
            AuthenticationRegion = region,
            ForcePathStyle = forcePathStyle,
            UseHttp = IsHttpEndpoint(endpoint)
        };
    }

    private static bool IsHttpEndpoint(string endpoint)
    {
        return Uri.TryCreate(endpoint, UriKind.Absolute, out var uri) &&
               uri.Scheme == Uri.UriSchemeHttp;
    }
}
