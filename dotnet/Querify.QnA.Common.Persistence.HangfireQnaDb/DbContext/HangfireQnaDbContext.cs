using Microsoft.EntityFrameworkCore;
using Querify.QnA.Common.Persistence.HangfireQnaDb.Configuration;

namespace Querify.QnA.Common.Persistence.HangfireQnaDb.DbContext;

public sealed class HangfireQnaDbContext(DbContextOptions<HangfireQnaDbContext> options)
    : Microsoft.EntityFrameworkCore.DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(HangfireQnaDbConfiguration.DefaultSchemaName);
    }
}
