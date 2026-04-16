using BaseFaq.QnA.Portal.Business.ThreadActivity.Abstractions;
using BaseFaq.QnA.Portal.Business.ThreadActivity.Queries.GetThreadActivity;
using BaseFaq.QnA.Portal.Business.ThreadActivity.Queries.GetThreadActivityList;
using BaseFaq.QnA.Portal.Business.ThreadActivity.Service;
using Microsoft.Extensions.DependencyInjection;

namespace BaseFaq.QnA.Portal.Business.ThreadActivity.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddThreadActivityBusiness(this IServiceCollection services)
    {
        services.AddScoped<IThreadActivityService, ThreadActivityService>();
        services.AddMediatR(config => config.RegisterServicesFromAssemblyContaining<ThreadActivitiesGetThreadActivityListQueryHandler>());

        return services;
    }
}
