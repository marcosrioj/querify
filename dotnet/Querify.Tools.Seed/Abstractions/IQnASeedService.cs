using Querify.QnA.Common.Persistence.QnADb.DbContext;
using Querify.Tools.Seed.Configuration;

namespace Querify.Tools.Seed.Abstractions;

public interface IQnASeedService
{
    bool HasData(QnADbContext dbContext);
    void Seed(QnADbContext dbContext, Guid tenantId, SeedCounts counts);
}
