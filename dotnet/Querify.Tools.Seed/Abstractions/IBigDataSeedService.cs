using Querify.QnA.Common.Persistence.QnADb.DbContext;
using Querify.Tools.Seed.Configuration;

namespace Querify.Tools.Seed.Abstractions;

public interface IBigDataSeedService
{
    bool HasData(QnADbContext dbContext);
    void Seed(QnADbContext dbContext, Guid tenantId, BigDataSeedSettings settings);
}
