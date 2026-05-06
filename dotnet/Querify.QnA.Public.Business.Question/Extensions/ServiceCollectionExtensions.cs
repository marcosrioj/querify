using Querify.QnA.Public.Business.Question.Abstractions;
using Querify.QnA.Public.Business.Question.Commands.CreateQuestion;
using Querify.QnA.Public.Business.Question.Service;
using Microsoft.Extensions.DependencyInjection;

namespace Querify.QnA.Public.Business.Question.Extensions;

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