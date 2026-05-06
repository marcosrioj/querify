using Querify.Models.Common.Dtos;
using Querify.Models.QnA.Dtos.Answer;
using Querify.QnA.Portal.Business.Answer.Abstractions;
using Querify.QnA.Portal.Business.Answer.Commands.AddSource;
using Querify.QnA.Portal.Business.Answer.Commands.ActivateAnswer;
using Querify.QnA.Portal.Business.Answer.Commands.ArchiveAnswer;
using Querify.QnA.Portal.Business.Answer.Commands.CreateAnswer;
using Querify.QnA.Portal.Business.Answer.Commands.DeleteAnswer;
using Querify.QnA.Portal.Business.Answer.Commands.RemoveSource;
using Querify.QnA.Portal.Business.Answer.Commands.UpdateAnswer;
using Querify.QnA.Portal.Business.Answer.Queries.GetAnswer;
using Querify.QnA.Portal.Business.Answer.Queries.GetAnswerList;
using MediatR;

namespace Querify.QnA.Portal.Business.Answer.Service;

public sealed class AnswerService(IMediator mediator) : IAnswerService
{
    public Task<Guid> Create(AnswerCreateRequestDto dto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return mediator.Send(new AnswersCreateAnswerCommand { Request = dto }, token);
    }

    public Task<PagedResultDto<AnswerDto>> GetAll(AnswerGetAllRequestDto requestDto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(requestDto);
        return mediator.Send(new AnswersGetAnswerListQuery { Request = requestDto }, token);
    }

    public Task<AnswerDto> GetById(Guid id, CancellationToken token)
    {
        return mediator.Send(new AnswersGetAnswerQuery { Id = id }, token);
    }

    public Task<Guid> Update(Guid id, AnswerUpdateRequestDto dto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return mediator.Send(new AnswersUpdateAnswerCommand { Id = id, Request = dto }, token);
    }

    public Task Delete(Guid id, CancellationToken token)
    {
        return mediator.Send(new AnswersDeleteAnswerCommand { Id = id }, token);
    }

    public Task<Guid> Activate(Guid id, CancellationToken token)
    {
        return mediator.Send(new AnswersActivateAnswerCommand { Id = id }, token);
    }

    public Task<Guid> Archive(Guid id, CancellationToken token)
    {
        return mediator.Send(new AnswersArchiveAnswerCommand { Id = id }, token);
    }

    public Task<Guid> AddSource(AnswerSourceLinkCreateRequestDto dto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return mediator.Send(new AnswersAddSourceCommand { Request = dto }, token);
    }

    public Task RemoveSource(Guid answerId, Guid sourceLinkId, CancellationToken token)
    {
        return mediator.Send(new AnswersRemoveSourceCommand
        {
            AnswerId = answerId,
            SourceLinkId = sourceLinkId
        }, token);
    }
}
