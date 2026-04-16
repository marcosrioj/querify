using BaseFaq.Faq.Public.Business.Faq.Extensions;
using BaseFaq.Faq.Public.Business.FaqItem.Extensions;
using BaseFaq.Faq.Public.Business.Feedback.Extensions;
using BaseFaq.Faq.Public.Business.Vote.Extensions;
using BaseFaq.Faq.Common.Persistence.FaqDb.Extensions;

namespace BaseFaq.Faq.Public.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddFeatures(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddFaqDb();
        services.AddFaqBusiness();
        services.AddFaqItemBusiness();
        services.AddFeedbackBusiness();
        services.AddVoteBusiness();
    }
}
