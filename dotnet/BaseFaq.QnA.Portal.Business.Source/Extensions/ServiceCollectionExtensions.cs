using BaseFaq.QnA.Portal.Business.Source.Abstractions;
using BaseFaq.QnA.Portal.Business.Source.Commands.CreateSource;
using BaseFaq.QnA.Portal.Business.Source.Service;
using Microsoft.Extensions.DependencyInjection;

namespace BaseFaq.QnA.Portal.Business.Source.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSourceBusiness(this IServiceCollection services)
    {
        services.AddScoped<ISourceService, SourceService>();
        services.AddMediatR(config =>
            config.RegisterServicesFromAssemblyContaining<SourcesCreateSourceCommandHandler>());

        return services;
    }
}