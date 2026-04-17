using BaseFaq.QnA.Common.Persistence.QnADb;
using BaseFaq.Tools.Seed.Configuration;

namespace BaseFaq.Tools.Seed.Abstractions;

public interface IQnASeedService
{
    bool HasData(QnADbContext dbContext);
    void Seed(QnADbContext dbContext, Guid tenantId, SeedCounts counts);
}
