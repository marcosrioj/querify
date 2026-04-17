using BaseFaq.QnA.Portal.Business.Tag.Abstractions;
using BaseFaq.QnA.Portal.Business.Tag.Commands.CreateTag;
using BaseFaq.QnA.Portal.Business.Tag.Service;
using Microsoft.Extensions.DependencyInjection;

namespace BaseFaq.QnA.Portal.Business.Tag.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTagBusiness(this IServiceCollection services)
    {
        services.AddScoped<ITagService, TagService>();
        services.AddMediatR(config => config.RegisterServicesFromAssemblyContaining<TagsCreateTagCommandHandler>());

        return services;
    }
}