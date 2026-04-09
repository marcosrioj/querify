using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Faq.Portal.Business.Vote.Abstractions;
using BaseFaq.Faq.Portal.Business.Vote.Commands.CreateVote;
using BaseFaq.Faq.Portal.Business.Vote.Commands.DeleteVote;
using BaseFaq.Faq.Portal.Business.Vote.Commands.UpdateVote;
using BaseFaq.Faq.Portal.Business.Vote.Queries.GetVote;
using BaseFaq.Faq.Portal.Business.Vote.Queries.GetVoteList;
using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Faq.Dtos.Vote;
using MediatR;

namespace BaseFaq.Faq.Portal.Business.Vote.Service;

public class VoteService(IMediator mediator) : IVoteService
{
    public async Task<Guid> Create(VoteCreateRequestDto requestDto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(requestDto);

        return await mediator.Send(new VotesCreateVoteCommand
        {
            FaqItemAnswerId = requestDto.FaqItemAnswerId
        }, token);
    }

    public Task Delete(Guid id, CancellationToken token)
    {
        return mediator.Send(new VotesDeleteVoteCommand { Id = id }, token);
    }

    public Task<PagedResultDto<VoteDto>> GetAll(VoteGetAllRequestDto requestDto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(requestDto);

        return mediator.Send(new VotesGetVoteListQuery { Request = requestDto }, token);
    }

    public async Task<VoteDto> GetById(Guid id, CancellationToken token)
    {
        var result = await mediator.Send(new VotesGetVoteQuery { Id = id }, token);
        if (result is null)
        {
            throw new ApiErrorException(
                $"Vote '{id}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        return result;
    }

    public async Task<Guid> Update(Guid id, VoteUpdateRequestDto requestDto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(requestDto);

        await mediator.Send(new VotesUpdateVoteCommand
        {
            Id = id,
            FaqItemAnswerId = requestDto.FaqItemAnswerId
        }, token);

        return id;
    }
}
