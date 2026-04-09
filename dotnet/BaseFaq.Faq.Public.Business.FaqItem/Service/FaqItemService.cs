using BaseFaq.Faq.Public.Business.FaqItem.Abstractions;
using BaseFaq.Faq.Public.Business.FaqItem.Commands.CreateFaqItem;
using BaseFaq.Faq.Public.Business.FaqItem.Queries.SearchFaqItem;
using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Faq.Dtos.FaqItem;
using MediatR;

namespace BaseFaq.Faq.Public.Business.FaqItem.Service;

public class FaqItemService(IMediator mediator) : IFaqItemService
{
    public async Task<Guid> Create(FaqItemCreateRequestDto requestDto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(requestDto);

        var command = new FaqItemsCreateFaqItemCommand
        {
            Question = requestDto.Question,
            AdditionalInfo = requestDto.AdditionalInfo,
            CtaTitle = requestDto.CtaTitle,
            CtaUrl = requestDto.CtaUrl,
            Sort = requestDto.Sort,
            IsActive = requestDto.IsActive,
            FaqId = requestDto.FaqId,
            ContentRefId = requestDto.ContentRefId
        };

        return await mediator.Send(command, token);
    }

    public Task<PagedResultDto<FaqItemDto>> Search(FaqItemSearchRequestDto requestDto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(requestDto);

        return mediator.Send(new FaqItemsSearchFaqItemQuery { Request = requestDto }, token);
    }
}
