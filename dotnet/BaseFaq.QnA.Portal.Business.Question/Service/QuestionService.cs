using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Dtos.Question;
using BaseFaq.QnA.Portal.Business.Question.Abstractions;
using BaseFaq.QnA.Portal.Business.Question.Commands;
using BaseFaq.QnA.Portal.Business.Question.Queries;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.Question.Service;

public sealed class QuestionService(IMediator mediator) : IQuestionService
{
    public Task<Guid> Create(QuestionCreateRequestDto dto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return mediator.Send(new QuestionsCreateQuestionCommand { Request = dto }, token);
    }

    public Task<PagedResultDto<QuestionDto>> GetAll(QuestionGetAllRequestDto requestDto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(requestDto);
        return mediator.Send(new QuestionsGetQuestionListQuery { Request = requestDto }, token);
    }

    public Task<QuestionDetailDto> GetById(Guid id, CancellationToken token)
    {
        return mediator.Send(new QuestionsGetQuestionQuery { Id = id }, token);
    }

    public Task<Guid> Update(Guid id, QuestionUpdateRequestDto dto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return mediator.Send(new QuestionsUpdateQuestionCommand { Id = id, Request = dto }, token);
    }

    public Task Delete(Guid id, CancellationToken token)
    {
        return mediator.Send(new QuestionsDeleteQuestionCommand { Id = id }, token);
    }

    public Task<Guid> Submit(Guid id, CancellationToken token)
    {
        return mediator.Send(new QuestionsSubmitQuestionCommand { Id = id }, token);
    }

    public Task<Guid> Approve(Guid id, CancellationToken token)
    {
        return mediator.Send(new QuestionsApproveQuestionCommand { Id = id }, token);
    }

    public Task<Guid> Reject(Guid id, string? notes, CancellationToken token)
    {
        return mediator.Send(new QuestionsRejectQuestionCommand { Id = id, Notes = notes }, token);
    }

    public Task<Guid> Escalate(Guid id, string? notes, CancellationToken token)
    {
        return mediator.Send(new QuestionsEscalateQuestionCommand { Id = id, Notes = notes }, token);
    }

    public Task<Guid> AddTopic(QuestionTopicCreateRequestDto dto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return mediator.Send(new QuestionsAddTopicCommand { Request = dto }, token);
    }

    public Task RemoveTopic(Guid questionId, Guid topicId, CancellationToken token)
    {
        return mediator.Send(new QuestionsRemoveTopicCommand
        {
            QuestionId = questionId,
            TopicId = topicId
        }, token);
    }

    public Task<Guid> AddSource(QuestionSourceLinkCreateRequestDto dto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return mediator.Send(new QuestionsAddSourceCommand { Request = dto }, token);
    }

    public Task RemoveSource(Guid questionId, Guid sourceLinkId, CancellationToken token)
    {
        return mediator.Send(new QuestionsRemoveSourceCommand
        {
            QuestionId = questionId,
            SourceLinkId = sourceLinkId
        }, token);
    }
}
