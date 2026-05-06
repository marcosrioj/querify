using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Querify.Common.EntityFramework.Core.AutoHistory.Attributes;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Net;

namespace Querify.Common.EntityFramework.Core.AutoHistory.DbContext.AutoHistory;

public static class AutoHistoryDbContextExtension
{
    /// <summary>
    ///     Ensures the automatic history.
    /// </summary>
    /// <param name="context">The context.</param>
    public static void EnsureAutoHistory(this Microsoft.EntityFrameworkCore.DbContext context)
    {
        EnsureAutoHistory(context, () => new Core.AutoHistory.AutoHistory());
    }

    public static void EnsureAutoHistory<TAutoHistory>(this Microsoft.EntityFrameworkCore.DbContext context,
        Func<TAutoHistory> createHistoryFactory)
        where TAutoHistory : Core.AutoHistory.AutoHistory
    {
        var entries = context.ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Where(e => !typeof(Core.AutoHistory.AutoHistory).IsAssignableFrom(e.Metadata.ClrType))
            .ToArray();
        foreach (var entry in entries)
        {
            var autoHistory = entry.AutoHistory(createHistoryFactory);
            if (autoHistory is not null)
            {
                context.Add(autoHistory);
            }
        }
    }

    internal static TAutoHistory? AutoHistory<TAutoHistory>(this EntityEntry entry,
        Func<TAutoHistory> createHistoryFactory)
        where TAutoHistory : Core.AutoHistory.AutoHistory
    {
        if (IsEntityExcluded(entry))
        {
            return null;
        }

        var properties = GetPropertiesWithoutExcluded(entry).ToList();

        if (entry.State == EntityState.Modified && !properties.Any(p => p.IsModified))
        {
            return null;
        }

        var history = createHistoryFactory();
        history.TableName = entry.Metadata.GetTableName()!;
        switch (entry.State)
        {
            case EntityState.Added:
                WriteHistoryAddedState(history, entry, properties);
                break;
            case EntityState.Modified:
                WriteHistoryModifiedState(history, entry, properties);
                break;
            case EntityState.Deleted:
                WriteHistoryDeletedState(history, entry, properties);
                break;
            case EntityState.Detached:
            case EntityState.Unchanged:
            default:
                throw new ApiErrorException(
                    "AutoHistory only supports Added, Modified, and Deleted entities.",
                    (int)HttpStatusCode.InternalServerError);
        }

        return history;
    }

    private static bool IsEntityExcluded(EntityEntry entry)
    {
        return entry.Metadata.ClrType.GetCustomAttributes(typeof(ExcludeFromHistoryAttribute), true).Any();
    }

    private static IEnumerable<PropertyEntry> GetPropertiesWithoutExcluded(EntityEntry entry)
    {
        // Get the mapped properties for the entity type.
        // (include shadow properties, not include navigations & references)
        var excludedProperties = entry.Metadata.ClrType.GetProperties()
            .Where(p => p.GetCustomAttributes(typeof(ExcludeFromHistoryAttribute), true).Any())
            .Select(p => p.Name);

        var properties = entry.Properties.Where(f => !excludedProperties.Contains(f.Metadata.Name));
        return properties;
    }

    public static void EnsureAddedHistory(
        this Microsoft.EntityFrameworkCore.DbContext context,
        EntityEntry[] addedEntries)
    {
        EnsureAddedHistory(
            context,
            () => new Core.AutoHistory.AutoHistory(),
            addedEntries);
    }

    public static void EnsureAddedHistory<TAutoHistory>(
        this Microsoft.EntityFrameworkCore.DbContext context,
        Func<TAutoHistory> createHistoryFactory,
        EntityEntry[] addedEntries)
        where TAutoHistory : Core.AutoHistory.AutoHistory
    {
        foreach (var entry in addedEntries)
        {
            var autoHistory = entry.AutoHistory(createHistoryFactory);
            if (autoHistory is not null)
            {
                context.Add(autoHistory);
            }
        }
    }

    internal static TAutoHistory? AddedHistory<TAutoHistory>(
        this EntityEntry entry,
        Func<TAutoHistory> createHistoryFactory)
        where TAutoHistory : Core.AutoHistory.AutoHistory
    {
        if (IsEntityExcluded(entry))
        {
            return null;
        }

        var history = createHistoryFactory();
        history.TableName = entry.Metadata.GetTableName()!;
        WriteHistoryAddedState(history, entry, GetPropertiesWithoutExcluded(entry));
        return history;
    }

    private static string PrimaryKey(this EntityEntry entry)
    {
        var key = entry.Metadata.FindPrimaryKey();

        var values = key!.Properties
            .Select(property => entry.Property(property.Name).CurrentValue)
            .Where(value => value != null)
            .ToList();

        return string.Join(",", values);
    }

    private static void WriteHistoryAddedState(Core.AutoHistory.AutoHistory history,
        EntityEntry entry,
        IEnumerable<PropertyEntry> properties)
    {
        dynamic json = new ExpandoObject();

        foreach (var prop in properties)
        {
            ((IDictionary<String, Object>)json)[prop.Metadata.Name] = prop.CurrentValue!;
        }

        history.KeyId = entry.PrimaryKey();
        history.Kind = EntityState.Added;
        history.ChangedTo = JsonSerializer.Serialize(json, AutoHistoryOption.Instance.JsonSerializerOptions);
    }

    private static void WriteHistoryModifiedState(Core.AutoHistory.AutoHistory history,
        EntityEntry entry,
        IEnumerable<PropertyEntry> properties)
    {
        dynamic bef = new ExpandoObject();
        dynamic aft = new ExpandoObject();

        PropertyValues? databaseValues = null;
        foreach (var prop in properties)
        {
            if (!prop.IsModified)
            {
                continue;
            }

            if (prop.OriginalValue != null)
            {
                if (!prop.OriginalValue.Equals(prop.CurrentValue))
                {
                    ((IDictionary<String, Object>)bef)[prop.Metadata.Name] = prop.OriginalValue;
                }
                else
                {
                    databaseValues ??= entry.GetDatabaseValues();
                    if (databaseValues != null)
                    {
                        var originalValue = databaseValues.GetValue<object>(prop.Metadata.Name);
                        ((IDictionary<String, Object>)bef)[prop.Metadata.Name] = originalValue;
                    }
                    else
                    {
                        ((IDictionary<String, Object>)bef)[prop.Metadata.Name] = prop.CurrentValue;
                    }
                }
            }
            else
            {
                ((IDictionary<String, Object>)bef)[prop.Metadata.Name] = null!;
            }

            ((IDictionary<String, Object>)aft)[prop.Metadata.Name] = prop.CurrentValue!;
        }

        //entry.Reload();

        history.KeyId = entry.PrimaryKey();
        history.Kind = EntityState.Modified;
        history.ChangedFrom = JsonSerializer.Serialize(bef, AutoHistoryOption.Instance.JsonSerializerOptions);
        history.ChangedTo = JsonSerializer.Serialize(aft, AutoHistoryOption.Instance.JsonSerializerOptions);
    }

    private static void WriteHistoryDeletedState(Core.AutoHistory.AutoHistory history,
        EntityEntry entry,
        IEnumerable<PropertyEntry> properties)
    {
        dynamic json = new ExpandoObject();

        foreach (var prop in properties)
        {
            ((IDictionary<String, Object>)json)[prop.Metadata.Name] = prop.OriginalValue!;
        }

        history.KeyId = entry.PrimaryKey();
        history.Kind = EntityState.Deleted;
        history.ChangedFrom = JsonSerializer.Serialize(json, AutoHistoryOption.Instance.JsonSerializerOptions);
    }

    public static JsonSerializerOptions Instance { get; } = new JsonSerializerOptions
    {
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
        WriteIndented = true // Optional: to make the JSON output more readable
    };
}
