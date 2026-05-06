using Querify.QnA.Common.Persistence.QnADb.DbContext;
using Microsoft.Extensions.DependencyInjection;

namespace Querify.QnA.Common.Persistence.QnADb.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddQnADb(this IServiceCollection services)
    {
        services.AddDbContext<QnADbContext>();

        return services;
    }
}
