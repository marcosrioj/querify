using Querify.Common.Infrastructure.MediatR.Logging;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Querify.Common.Infrastructure.MediatR.Extensions;

public static class MediatRLoggingServiceCollectionExtension
{
    public static void AddMediatRLogging(this IServiceCollection services)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    }
}