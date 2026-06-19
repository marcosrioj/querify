using Microsoft.Extensions.DependencyInjection;
using Querify.QnA.Portal.Business.SourceGeneration.Abstractions;
using Querify.QnA.Portal.Business.SourceGeneration.Commands.CreateSpaceGenerationRun;
using Querify.QnA.Portal.Business.SourceGeneration.Service;
using Querify.QnA.Worker.Business.SourceGeneration.Extensions;

namespace Querify.QnA.Portal.Business.SourceGeneration.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSourceGenerationBusiness(this IServiceCollection services)
    {
        services.AddScoped<ISourceGenerationService, SourceGenerationService>();
        services.AddMediatR(config =>
            config.RegisterServicesFromAssemblyContaining<SourcesCreateSpaceGenerationRunCommandHandler>());
        services.AddSourceGenerationExecutionBusiness();

        return services;
    }
}
