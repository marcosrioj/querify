using BaseFaq.QnA.Portal.Business.Activity.Abstractions;
using BaseFaq.QnA.Portal.Business.Activity.Queries.GetActivityList;
using BaseFaq.QnA.Portal.Business.Activity.Service;
using Microsoft.Extensions.DependencyInjection;

namespace BaseFaq.QnA.Portal.Business.Activity.Extensions;

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