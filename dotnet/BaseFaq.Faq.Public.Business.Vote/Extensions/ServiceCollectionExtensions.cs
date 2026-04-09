using BaseFaq.Faq.Public.Business.Vote.Abstractions;
using BaseFaq.Faq.Public.Business.Vote.Queries.GetVote;
using BaseFaq.Faq.Public.Business.Vote.Service;
using Microsoft.Extensions.DependencyInjection;

namespace BaseFaq.Faq.Public.Business.Vote.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVoteBusiness(this IServiceCollection services)
    {
        services.AddScoped<IVoteService, VoteService>();
        services.AddMediatR(config => config.RegisterServicesFromAssemblyContaining<VotesGetVoteQueryHandler>());

        return services;
    }
}
