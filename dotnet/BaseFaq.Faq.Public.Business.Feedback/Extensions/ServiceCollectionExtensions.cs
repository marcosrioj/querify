using BaseFaq.Faq.Public.Business.Feedback.Abstractions;
using BaseFaq.Faq.Public.Business.Feedback.Queries.GetFeedback;
using BaseFaq.Faq.Public.Business.Feedback.Service;
using Microsoft.Extensions.DependencyInjection;

namespace BaseFaq.Faq.Public.Business.Feedback.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFeedbackBusiness(this IServiceCollection services)
    {
        services.AddScoped<IFeedbackService, FeedbackService>();
        services.AddMediatR(config => config.RegisterServicesFromAssemblyContaining<FeedbacksGetFeedbackQueryHandler>());

        return services;
    }
}