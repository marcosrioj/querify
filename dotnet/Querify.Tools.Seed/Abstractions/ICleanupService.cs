using Querify.Common.EntityFramework.Tenant;
using Querify.QnA.Common.Persistence.QnADb.DbContext;

namespace Querify.Tools.Seed.Abstractions;

public interface ICleanupService
{
    void CleanTenantDb(TenantDbContext dbContext);
    void CleanQnADb(QnADbContext dbContext);
    void CleanBigDataQnADb(QnADbContext dbContext);
}
