using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Querify.Common.Infrastructure.Storage.Abstractions;
using Querify.Models.QnA.Enums;
using Querify.QnA.Common.Domain.BusinessRules.Sources;
using Querify.QnA.Common.Domain.Options;
using Querify.QnA.Common.Persistence.QnADb.DbContext;

namespace Querify.QnA.Worker.Business.Source.Commands.ExpirePendingSourceUploads;

public sealed class ExpirePendingSourceUploadsCommandHandler(
    QnADbContext dbContext,
    IObjectStorage objectStorage,
    IOptions<SourceUploadOptions> uploadOptions)
    : IRequestHandler<ExpirePendingSourceUploadsCommand, bool>
{
    private const string SystemUser = "system:qna-worker";

    public async Task<bool> Handle(ExpirePendingSourceUploadsCommand request, CancellationToken cancellationToken)
    {
        var cutoffUtc = request.NowUtc.AddHours(-uploadOptions.Value.PendingExpirationHours);
        var sources = await dbContext.Sources
            .Where(source => source.UploadStatus == SourceUploadStatus.Pending &&
                             source.CreatedDate != null &&
                             source.CreatedDate <= cutoffUtc)
            .OrderBy(source => source.CreatedDate)
            .ToListAsync(cancellationToken);

        foreach (var source in sources)
        {
            if (SourceStorageKey.IsStagingKey(source.StorageKey))
            {
                await objectStorage.DeleteAsync(source.StorageKey!, cancellationToken);
            }

            source.UploadStatus = SourceUploadStatus.Expired;
            source.UpdatedBy = SystemUser;
        }

        if (sources.Count > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return sources.Count > 0;
    }
}
