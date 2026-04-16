using BaseFaq.QnA.Public.Business.QuestionSpace.Abstractions;
using BaseFaq.QnA.Public.Business.QuestionSpace.Queries.GetQuestionSpace;
using BaseFaq.QnA.Public.Business.QuestionSpace.Queries.GetQuestionSpaceByKey;
using BaseFaq.QnA.Public.Business.QuestionSpace.Queries.GetQuestionSpaceList;
using BaseFaq.QnA.Public.Business.QuestionSpace.Service;
using Microsoft.Extensions.DependencyInjection;

namespace BaseFaq.QnA.Public.Business.QuestionSpace.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddQuestionSpaceBusiness(this IServiceCollection services)
    {
        services.AddScoped<IQuestionSpaceService, QuestionSpaceService>();
        services.AddMediatR(config => config.RegisterServicesFromAssemblyContaining<QuestionSpacesGetQuestionSpaceListQueryHandler>());

        return services;
    }
}
