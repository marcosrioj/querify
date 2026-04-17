using BaseFaq.QnA.Portal.Business.Space.Abstractions;
using BaseFaq.QnA.Portal.Business.Space.Commands.CreateSpace;
using BaseFaq.QnA.Portal.Business.Space.Service;
using Microsoft.Extensions.DependencyInjection;

namespace BaseFaq.QnA.Portal.Business.Space.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSpaceBusiness(this IServiceCollection services)
    {
        services.AddScoped<ISpaceService, SpaceService>();
        services.AddMediatR(config =>
            config.RegisterServicesFromAssemblyContaining<SpacesCreateSpaceCommandHandler>());

        return services;
    }
}