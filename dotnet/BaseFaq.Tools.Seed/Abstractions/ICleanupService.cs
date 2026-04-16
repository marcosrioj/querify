using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.QnA.Common.Persistence.QnADb;

namespace BaseFaq.Tools.Seed.Abstractions;

public interface ICleanupService
{
    void CleanTenantDb(TenantDbContext dbContext);
    void CleanQnADb(QnADbContext dbContext);
}
