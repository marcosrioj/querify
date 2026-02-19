using BaseFaq.Faq.Common.Persistence.FaqDb;

namespace BaseFaq.AI.Business.Common.Abstractions;

public interface IFaqDbContextFactory
{
    FaqDbContext Create(Guid tenantId);
}
