using Microsoft.Extensions.DependencyInjection;
using Querify.QnA.Public.Business.Search.Queries.Search;

namespace Querify.QnA.Public.Business.Search.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSearchBusiness(this IServiceCollection services)
    {
        services.AddMediatR(config =>
            config.RegisterServicesFromAssemblyContaining<QnASearchQueryHandler>());

        return services;
    }
}
