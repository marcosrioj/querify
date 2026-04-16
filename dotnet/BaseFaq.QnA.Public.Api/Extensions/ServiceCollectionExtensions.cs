using BaseFaq.QnA.Common.Persistence.QnADb.Extensions;
using BaseFaq.QnA.Public.Business.Feedback.Extensions;
using BaseFaq.QnA.Public.Business.Question.Extensions;
using BaseFaq.QnA.Public.Business.QuestionSpace.Extensions;
using BaseFaq.QnA.Public.Business.Vote.Extensions;

namespace BaseFaq.QnA.Public.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddFeatures(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddQnADb();
        services.AddFeedbackBusiness();
        services.AddQuestionBusiness();
        services.AddQuestionSpaceBusiness();
        services.AddVoteBusiness();
        services.AddEventsFeature(configuration);
    }
}