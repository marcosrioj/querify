using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Dtos.Answer;
using BaseFaq.QnA.Portal.Business.Answer.Abstractions;
using BaseFaq.QnA.Portal.Business.Answer.Commands.AddSource;
using BaseFaq.QnA.Portal.Business.Answer.Commands.CreateAnswer;
using BaseFaq.QnA.Portal.Business.Answer.Commands.DeleteAnswer;
using BaseFaq.QnA.Portal.Business.Answer.Commands.PublishAnswer;
using BaseFaq.QnA.Portal.Business.Answer.Commands.RejectAnswer;
using BaseFaq.QnA.Portal.Business.Answer.Commands.RemoveSource;
using BaseFaq.QnA.Portal.Business.Answer.Commands.RetireAnswer;
using BaseFaq.QnA.Portal.Business.Answer.Commands.UpdateAnswer;
using BaseFaq.QnA.Portal.Business.Answer.Commands.ValidateAnswer;
using BaseFaq.QnA.Portal.Business.Answer.Queries.GetAnswer;
using BaseFaq.QnA.Portal.Business.Answer.Queries.GetAnswerList;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Answer.Service;

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

    public Task<Guid> Publish(Guid id, CancellationToken token)
    {
        return mediator.Send(new AnswersPublishAnswerCommand { Id = id }, token);
    }

    public Task<Guid> Validate(Guid id, CancellationToken token)
    {
        return mediator.Send(new AnswersValidateAnswerCommand { Id = id }, token);
    }

    public Task<Guid> Reject(Guid id, CancellationToken token)
    {
        return mediator.Send(new AnswersRejectAnswerCommand { Id = id }, token);
    }

    public Task<Guid> Retire(Guid id, CancellationToken token)
    {
        return mediator.Send(new AnswersRetireAnswerCommand { Id = id }, token);
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