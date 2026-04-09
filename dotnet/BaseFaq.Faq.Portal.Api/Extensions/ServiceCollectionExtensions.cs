using BaseFaq.Faq.Portal.Business.ContentRef.Extensions;
using BaseFaq.Faq.Portal.Business.Faq.Extensions;
using BaseFaq.Faq.Portal.Business.FaqItem.Extensions;
using BaseFaq.Faq.Portal.Business.FaqItemAnswer.Extensions;
using BaseFaq.Faq.Portal.Business.Tag.Extensions;
using BaseFaq.Faq.Portal.Business.Feedback.Extensions;
using BaseFaq.Faq.Portal.Business.Vote.Extensions;
using BaseFaq.Faq.Common.Persistence.FaqDb.Extensions;

namespace BaseFaq.Faq.Portal.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddFeatures(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddFaqDb();
        services.AddFaqBusiness();
        services.AddFaqItemBusiness();
        services.AddFaqItemAnswerBusiness();
        services.AddTagBusiness();
        services.AddContentRefBusiness();
        services.AddFeedbackBusiness();
        services.AddVoteBusiness();
        services.AddEventsFeature(configuration);
    }
}
