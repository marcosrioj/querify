using Querify.QnA.Worker.Api.HostedServices;
using Querify.QnA.Worker.Business.Source.Extensions;

namespace Querify.QnA.Worker.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddQnAWorkerFeatures(this IServiceCollection services, IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddSourceWorker(configuration, environment);
        services.AddHostedService<SourceUploadHangfireJobRegistrationHostedService>();
    }
}
