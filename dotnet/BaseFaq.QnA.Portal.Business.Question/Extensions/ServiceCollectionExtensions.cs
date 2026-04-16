using BaseFaq.QnA.Portal.Business.Question.Abstractions;
using BaseFaq.QnA.Portal.Business.Question.Commands.AddSource;
using BaseFaq.QnA.Portal.Business.Question.Commands.AddTopic;
using BaseFaq.QnA.Portal.Business.Question.Commands.ApproveQuestion;
using BaseFaq.QnA.Portal.Business.Question.Commands.CreateQuestion;
using BaseFaq.QnA.Portal.Business.Question.Commands.DeleteQuestion;
using BaseFaq.QnA.Portal.Business.Question.Commands.EscalateQuestion;
using BaseFaq.QnA.Portal.Business.Question.Commands.RejectQuestion;
using BaseFaq.QnA.Portal.Business.Question.Commands.RemoveSource;
using BaseFaq.QnA.Portal.Business.Question.Commands.RemoveTopic;
using BaseFaq.QnA.Portal.Business.Question.Commands.SubmitQuestion;
using BaseFaq.QnA.Portal.Business.Question.Commands.UpdateQuestion;
using BaseFaq.QnA.Portal.Business.Question.Service;
using Microsoft.Extensions.DependencyInjection;

namespace BaseFaq.QnA.Portal.Business.Question.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddQuestionBusiness(this IServiceCollection services)
    {
        services.AddScoped<IQuestionService, QuestionService>();
        services.AddMediatR(config => config.RegisterServicesFromAssemblyContaining<QuestionsCreateQuestionCommandHandler>());

        return services;
    }
}
