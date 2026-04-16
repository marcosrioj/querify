using BaseFaq.QnA.Portal.Business.Topic.Abstractions;
using BaseFaq.QnA.Portal.Business.Topic.Commands.CreateTopic;
using BaseFaq.QnA.Portal.Business.Topic.Service;
using Microsoft.Extensions.DependencyInjection;

namespace BaseFaq.QnA.Portal.Business.Topic.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTopicBusiness(this IServiceCollection services)
    {
        services.AddScoped<ITopicService, TopicService>();
        services.AddMediatR(config => config.RegisterServicesFromAssemblyContaining<TopicsCreateTopicCommandHandler>());

        return services;
    }
}