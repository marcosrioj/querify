using Querify.Common.Infrastructure.Storage.Extensions;
using Querify.QnA.Common.Domain.Options;
using Querify.QnA.Common.Persistence.QnADb.Extensions;
using Querify.QnA.Portal.Business.Activity.Extensions;
using Querify.QnA.Portal.Business.Answer.Extensions;
using Querify.QnA.Portal.Business.Question.Extensions;
using Querify.QnA.Portal.Business.Source.Extensions;
using Querify.QnA.Portal.Business.Space.Extensions;
using Querify.QnA.Portal.Business.Tag.Extensions;

namespace Querify.QnA.Portal.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddFeatures(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddQnADb();
        services.AddObjectStorage(configuration);
        services.AddOptions<SourceUploadOptions>()
            .BindConfiguration(SourceUploadOptions.SectionName)
            .Validate(options => options.MaxUploadBytes > 0, "Source upload max size must be greater than zero.")
            .Validate(options => options.PendingExpirationHours > 0,
                "Source upload pending expiration hours must be greater than zero.")
            .Validate(options => options.AllowedContentTypes.Length > 0,
                "At least one source upload content type must be allowed.")
            .ValidateOnStart();
        services.AddAnswerBusiness();
        services.AddSourceBusiness();
        services.AddQuestionBusiness();
        services.AddSpaceBusiness();
        services.AddActivityBusiness();
        services.AddTagBusiness();
        services.AddEventsFeature(configuration);
    }
}
