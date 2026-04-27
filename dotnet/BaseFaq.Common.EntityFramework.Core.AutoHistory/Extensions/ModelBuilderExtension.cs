using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Common.EntityFramework.Core.AutoHistory.Extensions;

public static class ModelBuilderExtension
{
    private const int DefaultChangedMaxLength = 2048;

    public static ModelBuilder EnableAutoHistory(this ModelBuilder modelBuilder, int? changedMaxLength = null)
    {
        return modelBuilder.EnableAutoHistory<AutoHistory>(o =>
        {
            o.ChangedMaxLength = changedMaxLength;
            o.LimitChangedLength = false;
        });
    }

    public static ModelBuilder EnableAutoHistory<TAutoHistory>(this ModelBuilder modelBuilder,
        Action<AutoHistoryOption> configure)
        where TAutoHistory : AutoHistory
    {
        var options = AutoHistoryOption.Instance;
        configure?.Invoke(options);

        modelBuilder.Entity<TAutoHistory>(b =>
        {
            b.ToTable(options.TableName);
            b.Property(c => c.KeyId).IsRequired().HasMaxLength(options.RowIdMaxLength);
            b.Property(c => c.TableName).IsRequired().HasMaxLength(options.TableMaxLength);

            if (options.LimitChangedLength)
            {
                var max = options.ChangedMaxLength ?? DefaultChangedMaxLength;
                if (max <= 0)
                {
                    max = DefaultChangedMaxLength;
                }

                b.Property(c => c.ChangedFrom).HasMaxLength(max);
                b.Property(c => c.ChangedTo).HasMaxLength(max);
            }
        });

        return modelBuilder;
    }
}