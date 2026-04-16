using BaseFaq.QnA.Common.Persistence.QnADb.Extensions;
using BaseFaq.QnA.Portal.Business.Answer.Extensions;
using BaseFaq.QnA.Portal.Business.Source.Extensions;
using BaseFaq.QnA.Portal.Business.Question.Extensions;
using BaseFaq.QnA.Portal.Business.Space.Extensions;
using BaseFaq.QnA.Portal.Business.Activity.Extensions;
using BaseFaq.QnA.Portal.Business.Tag.Extensions;

namespace BaseFaq.QnA.Portal.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddFeatures(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddQnADb();
        services.AddAnswerBusiness();
        services.AddSourceBusiness();
        services.AddQuestionBusiness();
        services.AddSpaceBusiness();
        services.AddActivityBusiness();
        services.AddTagBusiness();
        services.AddEventsFeature(configuration);
    }
}