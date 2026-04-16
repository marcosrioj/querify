using BaseFaq.QnA.Portal.Business.KnowledgeSource.Abstractions;
using BaseFaq.QnA.Portal.Business.KnowledgeSource.Commands.CreateKnowledgeSource;
using BaseFaq.QnA.Portal.Business.KnowledgeSource.Commands.DeleteKnowledgeSource;
using BaseFaq.QnA.Portal.Business.KnowledgeSource.Commands.UpdateKnowledgeSource;
using BaseFaq.QnA.Portal.Business.KnowledgeSource.Service;
using Microsoft.Extensions.DependencyInjection;

namespace BaseFaq.QnA.Portal.Business.KnowledgeSource.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKnowledgeSourceBusiness(this IServiceCollection services)
    {
        services.AddScoped<IKnowledgeSourceService, KnowledgeSourceService>();
        services.AddMediatR(config => config.RegisterServicesFromAssemblyContaining<KnowledgeSourcesCreateKnowledgeSourceCommandHandler>());

        return services;
    }
}
