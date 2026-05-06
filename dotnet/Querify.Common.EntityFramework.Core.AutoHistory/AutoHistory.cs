using Microsoft.EntityFrameworkCore;

namespace Querify.Common.EntityFramework.Core.AutoHistory;

public class AutoHistory
{
    /// <summary>
    ///     Gets or sets the primary key.
    /// </summary>
    /// <value>The id.</value>
    public Guid Id { get; set; }

    /// <summary>
    ///     Gets or sets the source row id.
    /// </summary>
    /// <value>The source row id.</value>
    public string KeyId { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the name of the table.
    /// </summary>
    /// <value>The name of the table.</value>
    public string TableName { get; set; }= string.Empty;

    /// <summary>
    ///     Gets or sets the json about the changing from.
    /// </summary>
    /// <value>The json about the changing from.</value>
    public string? ChangedFrom { get; set; }

    /// <summary>
    ///     Gets or sets the json about the changing to.
    /// </summary>
    /// <value>The json about the changing to.</value>
    public string? ChangedTo { get; set; }

    /// <summary>
    ///     Gets or sets the change kind.
    /// </summary>
    /// <value>The change kind.</value>
    public EntityState Kind { get; set; }

    /// <summary>
    ///     Gets or sets the create time.
    /// </summary>
    /// <value>The create time.</value>
    public DateTime Created { get; set; } = DateTime.UtcNow;
}