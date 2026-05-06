using Querify.QnA.Public.Business.Vote.Abstractions;
using Querify.QnA.Public.Business.Vote.Commands.CreateVote;
using Querify.QnA.Public.Business.Vote.Service;
using Microsoft.Extensions.DependencyInjection;

namespace Querify.QnA.Public.Business.Vote.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVoteBusiness(this IServiceCollection services)
    {
        services.AddScoped<IVoteService, VoteService>();
        services.AddMediatR(config => config.RegisterServicesFromAssemblyContaining<VotesCreateVoteCommandHandler>());

        return services;
    }
}