using BaseFaq.QnA.Common.Persistence.QnADb.DbContext;
using BaseFaq.Tools.Seed.Configuration;

namespace BaseFaq.Tools.Seed.Abstractions;

public interface IBigDataSeedService
{
    bool HasData(QnADbContext dbContext);
    void Seed(QnADbContext dbContext, Guid tenantId, BigDataSeedSettings settings);
}
