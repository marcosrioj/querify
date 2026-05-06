using Querify.QnA.Portal.Business.Answer.Abstractions;
using Querify.QnA.Portal.Business.Answer.Commands.CreateAnswer;
using Querify.QnA.Portal.Business.Answer.Service;
using Microsoft.Extensions.DependencyInjection;

namespace Querify.QnA.Portal.Business.Answer.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAnswerBusiness(this IServiceCollection services)
    {
        services.AddScoped<IAnswerService, AnswerService>();
        services.AddMediatR(config =>
            config.RegisterServicesFromAssemblyContaining<AnswersCreateAnswerCommandHandler>());

        return services;
    }
}