using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Dtos.Question;
using BaseFaq.QnA.Public.Business.Question.Abstractions;
using BaseFaq.QnA.Public.Business.Question.Commands;
using BaseFaq.QnA.Public.Business.Question.Queries;
using MediatR;

namespace BaseFaq.QnA.Public.Business.Question.Service;

public sealed class QuestionService(IMediator mediator) : IQuestionService
{
    public Task<PagedResultDto<QuestionDto>> GetAll(QuestionGetAllRequestDto requestDto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(requestDto);
        return mediator.Send(new QuestionsGetQuestionListQuery { Request = requestDto }, token);
    }

    public Task<QuestionDetailDto> GetById(Guid id, QuestionGetRequestDto requestDto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(requestDto);
        return mediator.Send(new QuestionsGetQuestionQuery
        {
            Id = id,
            Request = requestDto
        }, token);
    }

    public Task<QuestionDetailDto> GetByKey(string key, QuestionGetRequestDto requestDto, CancellationToken token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(requestDto);
        return mediator.Send(new QuestionsGetQuestionByKeyQuery
        {
            Key = key,
            Request = requestDto
        }, token);
    }

    public Task<Guid> Create(QuestionCreateRequestDto dto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return mediator.Send(new QuestionsCreateQuestionCommand { Request = dto }, token);
    }
}
