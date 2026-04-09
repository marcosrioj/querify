using BaseFaq.Faq.Portal.Business.Vote.Abstractions;
using BaseFaq.Faq.Portal.Business.Vote.Queries.GetVote;
using BaseFaq.Faq.Portal.Business.Vote.Service;
using Microsoft.Extensions.DependencyInjection;

namespace BaseFaq.Faq.Portal.Business.Vote.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVoteBusiness(this IServiceCollection services)
    {
        services.AddScoped<IVoteService, VoteService>();
        services.AddMediatR(config => config.RegisterServicesFromAssemblyContaining<VotesGetVoteQueryHandler>());

        return services;
    }
}
