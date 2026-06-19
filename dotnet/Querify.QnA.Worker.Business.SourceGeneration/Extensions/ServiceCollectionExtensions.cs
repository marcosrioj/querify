using Microsoft.Extensions.DependencyInjection;
using Querify.QnA.Worker.Business.SourceGeneration.Commands.ExecuteSpaceGenerationRun;

namespace Querify.QnA.Worker.Business.SourceGeneration.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSourceGenerationExecutionBusiness(this IServiceCollection services)
    {
        services.AddMediatR(config =>
            config.RegisterServicesFromAssemblyContaining<SourcesExecuteSpaceGenerationRunCommandHandler>());

        return services;
    }
}
