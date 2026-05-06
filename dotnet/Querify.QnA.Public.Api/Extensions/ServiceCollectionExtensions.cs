using Querify.QnA.Common.Persistence.QnADb.Extensions;
using Querify.QnA.Public.Business.Feedback.Extensions;
using Querify.QnA.Public.Business.Question.Extensions;
using Querify.QnA.Public.Business.Space.Extensions;
using Querify.QnA.Public.Business.Vote.Extensions;

namespace Querify.QnA.Public.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddFeatures(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddQnADb();
        services.AddFeedbackBusiness();
        services.AddQuestionBusiness();
        services.AddSpaceBusiness();
        services.AddVoteBusiness();
        services.AddEventsFeature(configuration);
    }
}