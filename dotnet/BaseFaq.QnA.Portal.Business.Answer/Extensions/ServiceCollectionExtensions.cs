using BaseFaq.QnA.Portal.Business.Answer.Abstractions;
using BaseFaq.QnA.Portal.Business.Answer.Commands.AddSource;
using BaseFaq.QnA.Portal.Business.Answer.Commands.CreateAnswer;
using BaseFaq.QnA.Portal.Business.Answer.Commands.DeleteAnswer;
using BaseFaq.QnA.Portal.Business.Answer.Commands.PublishAnswer;
using BaseFaq.QnA.Portal.Business.Answer.Commands.RejectAnswer;
using BaseFaq.QnA.Portal.Business.Answer.Commands.RemoveSource;
using BaseFaq.QnA.Portal.Business.Answer.Commands.RetireAnswer;
using BaseFaq.QnA.Portal.Business.Answer.Commands.UpdateAnswer;
using BaseFaq.QnA.Portal.Business.Answer.Commands.ValidateAnswer;
using BaseFaq.QnA.Portal.Business.Answer.Service;
using Microsoft.Extensions.DependencyInjection;

namespace BaseFaq.QnA.Portal.Business.Answer.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAnswerBusiness(this IServiceCollection services)
    {
        services.AddScoped<IAnswerService, AnswerService>();
        services.AddMediatR(config => config.RegisterServicesFromAssemblyContaining<AnswersCreateAnswerCommandHandler>());

        return services;
    }
}
