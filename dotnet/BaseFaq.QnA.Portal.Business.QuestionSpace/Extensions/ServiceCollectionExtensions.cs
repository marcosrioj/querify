using BaseFaq.QnA.Portal.Business.QuestionSpace.Abstractions;
using BaseFaq.QnA.Portal.Business.QuestionSpace.Commands.AddCuratedSource;
using BaseFaq.QnA.Portal.Business.QuestionSpace.Commands.AddTopic;
using BaseFaq.QnA.Portal.Business.QuestionSpace.Commands.CreateQuestionSpace;
using BaseFaq.QnA.Portal.Business.QuestionSpace.Commands.DeleteQuestionSpace;
using BaseFaq.QnA.Portal.Business.QuestionSpace.Commands.RemoveCuratedSource;
using BaseFaq.QnA.Portal.Business.QuestionSpace.Commands.RemoveTopic;
using BaseFaq.QnA.Portal.Business.QuestionSpace.Commands.UpdateQuestionSpace;
using BaseFaq.QnA.Portal.Business.QuestionSpace.Service;
using Microsoft.Extensions.DependencyInjection;

namespace BaseFaq.QnA.Portal.Business.QuestionSpace.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddQuestionSpaceBusiness(this IServiceCollection services)
    {
        services.AddScoped<IQuestionSpaceService, QuestionSpaceService>();
        services.AddMediatR(config => config.RegisterServicesFromAssemblyContaining<QuestionSpacesCreateQuestionSpaceCommandHandler>());

        return services;
    }
}
