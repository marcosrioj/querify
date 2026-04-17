using BaseFaq.QnA.Public.Business.Space.Abstractions;
using BaseFaq.QnA.Public.Business.Space.Queries.GetSpaceList;
using BaseFaq.QnA.Public.Business.Space.Service;
using Microsoft.Extensions.DependencyInjection;

namespace BaseFaq.QnA.Public.Business.Space.Extensions;

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