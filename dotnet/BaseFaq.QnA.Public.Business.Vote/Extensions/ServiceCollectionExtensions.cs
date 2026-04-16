using BaseFaq.QnA.Public.Business.Vote.Abstractions;
using BaseFaq.QnA.Public.Business.Vote.Commands.CreateVote;
using BaseFaq.QnA.Public.Business.Vote.Service;
using Microsoft.Extensions.DependencyInjection;

namespace BaseFaq.QnA.Public.Business.Vote.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVoteBusiness(this IServiceCollection services)
    {
        services.AddScoped<IVoteService, VoteService>();
        services.AddMediatR(config => config.RegisterServicesFromAssemblyContaining<VotesCreateVoteCommandHandler>());

        return services;
    }
}