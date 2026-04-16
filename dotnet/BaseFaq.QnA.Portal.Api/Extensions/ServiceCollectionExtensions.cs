using BaseFaq.QnA.Common.Persistence.QnADb.Extensions;
using BaseFaq.QnA.Portal.Business.Answer.Extensions;
using BaseFaq.QnA.Portal.Business.KnowledgeSource.Extensions;
using BaseFaq.QnA.Portal.Business.Question.Extensions;
using BaseFaq.QnA.Portal.Business.QuestionSpace.Extensions;
using BaseFaq.QnA.Portal.Business.ThreadActivity.Extensions;
using BaseFaq.QnA.Portal.Business.Tag.Extensions;

namespace BaseFaq.QnA.Portal.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddFeatures(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddQnADb();
        services.AddAnswerBusiness();
        services.AddKnowledgeSourceBusiness();
        services.AddQuestionBusiness();
        services.AddQuestionSpaceBusiness();
        services.AddThreadActivityBusiness();
        services.AddTagBusiness();
        services.AddEventsFeature(configuration);
    }
}