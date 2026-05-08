using Querify.Models.Common.Dtos;
using Querify.Models.QnA.Dtos.Source;

namespace Querify.QnA.Portal.Business.Source.Abstractions;

public interface ISourceService
{
    Task<Guid> Create(SourceCreateRequestDto dto, CancellationToken token);
    Task<SourceUploadIntentResponseDto> CreateUploadIntent(SourceUploadIntentRequestDto dto,
        CancellationToken token);
    Task<Guid> CompleteUpload(Guid sourceId, SourceUploadCompleteRequestDto dto, CancellationToken token);
    Task Delete(Guid id, CancellationToken token);
    Task<SourceDownloadUrlDto> GetDownloadUrl(Guid id, CancellationToken token);
    Task<SourceExternalUrlInspectionDto> InspectExternalUrl(SourceExternalUrlInspectionRequestDto dto,
        CancellationToken token);

    Task<PagedResultDto<SourceDto>>
        GetAll(SourceGetAllRequestDto requestDto, CancellationToken token);

    Task<SourceDetailDto> GetById(Guid id, CancellationToken token);
    Task<Guid> Update(Guid id, SourceUpdateRequestDto dto, CancellationToken token);
}
