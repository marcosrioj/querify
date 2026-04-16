using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Dtos.QuestionSpace;
using BaseFaq.QnA.Public.Business.QuestionSpace.Abstractions;
using BaseFaq.QnA.Public.Business.QuestionSpace.Queries.GetQuestionSpace;
using BaseFaq.QnA.Public.Business.QuestionSpace.Queries.GetQuestionSpaceByKey;
using BaseFaq.QnA.Public.Business.QuestionSpace.Queries.GetQuestionSpaceList;
using MediatR;

namespace BaseFaq.QnA.Public.Business.QuestionSpace.Service;

public sealed class QuestionSpaceService(IMediator mediator) : IQuestionSpaceService
{
    public Task<PagedResultDto<QuestionSpaceDto>> GetAll(QuestionSpaceGetAllRequestDto requestDto,
        CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(requestDto);
        return mediator.Send(new QuestionSpacesGetQuestionSpaceListQuery { Request = requestDto }, token);
    }

    public Task<QuestionSpaceDto> GetById(Guid id, CancellationToken token)
    {
        return mediator.Send(new QuestionSpacesGetQuestionSpaceQuery { Id = id }, token);
    }

    public Task<QuestionSpaceDto> GetByKey(string key, CancellationToken token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return mediator.Send(new QuestionSpacesGetQuestionSpaceByKeyQuery { Key = key }, token);
    }
}