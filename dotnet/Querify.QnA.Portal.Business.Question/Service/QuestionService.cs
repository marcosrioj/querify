using Querify.Models.Common.Dtos;
using Querify.Models.QnA.Dtos.Question;
using Querify.QnA.Portal.Business.Question.Abstractions;
using Querify.QnA.Portal.Business.Question.Commands.AddSource;
using Querify.QnA.Portal.Business.Question.Commands.AddTag;
using Querify.QnA.Portal.Business.Question.Commands.CreateQuestion;
using Querify.QnA.Portal.Business.Question.Commands.DeleteQuestion;
using Querify.QnA.Portal.Business.Question.Commands.RemoveSource;
using Querify.QnA.Portal.Business.Question.Commands.RemoveTag;
using Querify.QnA.Portal.Business.Question.Commands.UpdateQuestion;
using Querify.QnA.Portal.Business.Question.Queries.GetQuestion;
using Querify.QnA.Portal.Business.Question.Queries.GetQuestionList;
using MediatR;

namespace Querify.QnA.Portal.Business.Question.Service;

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

    public Task<Guid> AddTag(QuestionTagCreateRequestDto dto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return mediator.Send(new QuestionsAddTagCommand { Request = dto }, token);
    }

    public Task RemoveTag(Guid questionId, Guid tagId, CancellationToken token)
    {
        return mediator.Send(new QuestionsRemoveTagCommand
        {
            QuestionId = questionId,
            TagId = tagId
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
