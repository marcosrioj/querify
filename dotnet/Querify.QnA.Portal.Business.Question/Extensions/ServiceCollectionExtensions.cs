using Querify.QnA.Portal.Business.Question.Abstractions;
using Querify.QnA.Portal.Business.Question.Commands.CreateQuestion;
using Querify.QnA.Portal.Business.Question.Service;
using Microsoft.Extensions.DependencyInjection;

namespace Querify.QnA.Portal.Business.Question.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddQuestionBusiness(this IServiceCollection services)
    {
        services.AddScoped<IQuestionService, QuestionService>();
        services.AddMediatR(config =>
            config.RegisterServicesFromAssemblyContaining<QuestionsCreateQuestionCommandHandler>());

        return services;
    }
}