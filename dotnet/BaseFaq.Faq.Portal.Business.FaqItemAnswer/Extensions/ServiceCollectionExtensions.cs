using BaseFaq.Faq.Portal.Business.FaqItemAnswer.Commands.CreateFaqItemAnswer;
using Microsoft.Extensions.DependencyInjection;

namespace BaseFaq.Faq.Portal.Business.FaqItemAnswer.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFaqItemAnswerBusiness(this IServiceCollection services)
    {
        services.AddMediatR(config =>
            config.RegisterServicesFromAssemblyContaining<FaqItemAnswersCreateFaqItemAnswerCommandHandler>());

        return services;
    }
}
