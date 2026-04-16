using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Dtos.QuestionSpace;
using BaseFaq.QnA.Portal.Business.QuestionSpace.Abstractions;
using BaseFaq.QnA.Portal.Business.QuestionSpace.Commands.AddCuratedSource;
using BaseFaq.QnA.Portal.Business.QuestionSpace.Commands.AddTopic;
using BaseFaq.QnA.Portal.Business.QuestionSpace.Commands.CreateQuestionSpace;
using BaseFaq.QnA.Portal.Business.QuestionSpace.Commands.DeleteQuestionSpace;
using BaseFaq.QnA.Portal.Business.QuestionSpace.Commands.RemoveCuratedSource;
using BaseFaq.QnA.Portal.Business.QuestionSpace.Commands.RemoveTopic;
using BaseFaq.QnA.Portal.Business.QuestionSpace.Commands.UpdateQuestionSpace;
using BaseFaq.QnA.Portal.Business.QuestionSpace.Queries.GetQuestionSpace;
using BaseFaq.QnA.Portal.Business.QuestionSpace.Queries.GetQuestionSpaceList;
using MediatR;

namespace BaseFaq.QnA.Portal.Business.QuestionSpace.Service;

public sealed class QuestionSpaceService(IMediator mediator) : IQuestionSpaceService
{
    public Task<Guid> Create(QuestionSpaceCreateRequestDto dto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return mediator.Send(new QuestionSpacesCreateQuestionSpaceCommand { Request = dto }, token);
    }

    public Task<PagedResultDto<QuestionSpaceDto>> GetAll(QuestionSpaceGetAllRequestDto requestDto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(requestDto);
        return mediator.Send(new QuestionSpacesGetQuestionSpaceListQuery { Request = requestDto }, token);
    }

    public Task<QuestionSpaceDetailDto> GetById(Guid id, CancellationToken token)
    {
        return mediator.Send(new QuestionSpacesGetQuestionSpaceQuery { Id = id }, token);
    }

    public Task<Guid> Update(Guid id, QuestionSpaceUpdateRequestDto dto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return mediator.Send(new QuestionSpacesUpdateQuestionSpaceCommand { Id = id, Request = dto }, token);
    }

    public Task Delete(Guid id, CancellationToken token)
    {
        return mediator.Send(new QuestionSpacesDeleteQuestionSpaceCommand { Id = id }, token);
    }

    public Task<Guid> AddTopic(QuestionSpaceTopicCreateRequestDto dto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return mediator.Send(new QuestionSpacesAddTopicCommand { Request = dto }, token);
    }

    public Task RemoveTopic(Guid questionSpaceId, Guid topicId, CancellationToken token)
    {
        return mediator.Send(new QuestionSpacesRemoveTopicCommand
        {
            QuestionSpaceId = questionSpaceId,
            TopicId = topicId
        }, token);
    }

    public Task<Guid> AddCuratedSource(QuestionSpaceSourceCreateRequestDto dto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return mediator.Send(new QuestionSpacesAddCuratedSourceCommand { Request = dto }, token);
    }

    public Task RemoveCuratedSource(Guid questionSpaceId, Guid knowledgeSourceId, CancellationToken token)
    {
        return mediator.Send(new QuestionSpacesRemoveCuratedSourceCommand
        {
            QuestionSpaceId = questionSpaceId,
            KnowledgeSourceId = knowledgeSourceId
        }, token);
    }
}
