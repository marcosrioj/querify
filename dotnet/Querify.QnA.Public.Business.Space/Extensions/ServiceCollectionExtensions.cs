using Querify.QnA.Public.Business.Space.Abstractions;
using Querify.QnA.Public.Business.Space.Queries.GetSpaceList;
using Querify.QnA.Public.Business.Space.Service;
using Microsoft.Extensions.DependencyInjection;

namespace Querify.QnA.Public.Business.Space.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSpaceBusiness(this IServiceCollection services)
    {
        services.AddScoped<ISpaceService, SpaceService>();
        services.AddMediatR(config =>
            config.RegisterServicesFromAssemblyContaining<SpacesGetSpaceListQueryHandler>());

        return services;
    }
}