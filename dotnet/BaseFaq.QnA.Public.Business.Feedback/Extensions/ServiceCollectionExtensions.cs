using BaseFaq.QnA.Public.Business.Feedback.Abstractions;
using BaseFaq.QnA.Public.Business.Feedback.Commands.CreateFeedback;
using BaseFaq.QnA.Public.Business.Feedback.Service;
using Microsoft.Extensions.DependencyInjection;

namespace BaseFaq.QnA.Public.Business.Feedback.Extensions;

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