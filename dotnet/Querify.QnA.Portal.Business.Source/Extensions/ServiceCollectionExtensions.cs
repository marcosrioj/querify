using Querify.QnA.Portal.Business.Source.Abstractions;
using Querify.QnA.Portal.Business.Source.Commands.CreateSource;
using Querify.QnA.Portal.Business.Source.Events;
using Querify.QnA.Portal.Business.Source.Notifications;
using Querify.QnA.Portal.Business.Source.Queries.InspectExternalUrl;
using Querify.QnA.Portal.Business.Source.Service;
using Microsoft.Extensions.DependencyInjection;

namespace Querify.QnA.Portal.Business.Source.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSourceBusiness(this IServiceCollection services)
    {
        services.AddScoped<ISourceService, SourceService>();
        services.AddScoped<ISourceUploadCompletedEventPublisher, MassTransitSourceUploadCompletedEventPublisher>();
        services.AddScoped<ISourceUploadStatusChangedNotificationService, SourceUploadStatusChangedNotificationService>();
        services
            .AddHttpClient(SourcesInspectExternalUrlQueryHandler.HttpClientName)
            .ConfigureHttpClient(client => client.Timeout = TimeSpan.FromSeconds(10))
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                AllowAutoRedirect = false
            });
        services.AddMediatR(config =>
            config.RegisterServicesFromAssemblyContaining<SourcesCreateSourceCommandHandler>());

        return services;
    }
}
