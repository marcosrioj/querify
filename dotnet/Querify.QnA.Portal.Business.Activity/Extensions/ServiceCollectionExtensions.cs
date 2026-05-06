using Querify.QnA.Portal.Business.Activity.Abstractions;
using Querify.QnA.Portal.Business.Activity.Queries.GetActivityList;
using Querify.QnA.Portal.Business.Activity.Service;
using Microsoft.Extensions.DependencyInjection;

namespace Querify.QnA.Portal.Business.Activity.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddActivityBusiness(this IServiceCollection services)
    {
        services.AddScoped<IActivityService, ActivityService>();
        services.AddMediatR(config =>
            config.RegisterServicesFromAssemblyContaining<ActivitiesGetActivityListQueryHandler>());

        return services;
    }
}