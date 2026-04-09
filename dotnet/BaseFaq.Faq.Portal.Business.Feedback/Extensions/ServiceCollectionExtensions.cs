using BaseFaq.Faq.Portal.Business.Feedback.Abstractions;
using BaseFaq.Faq.Portal.Business.Feedback.Commands.CreateFeedback;
using BaseFaq.Faq.Portal.Business.Feedback.Service;
using Microsoft.Extensions.DependencyInjection;

namespace BaseFaq.Faq.Portal.Business.Feedback.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFeedbackBusiness(this IServiceCollection services)
    {
        services.AddScoped<IFeedbackService, FeedbackService>();
        services.AddMediatR(config =>
            config.RegisterServicesFromAssemblyContaining<FeedbacksCreateFeedbackCommandHandler>());

        return services;
    }
}