using BaseFaq.QnA.Public.Business.Question.Abstractions;
using BaseFaq.QnA.Public.Business.Question.Commands.CreateQuestion;
using BaseFaq.QnA.Public.Business.Question.Service;
using Microsoft.Extensions.DependencyInjection;

namespace BaseFaq.QnA.Public.Business.Question.Extensions;

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