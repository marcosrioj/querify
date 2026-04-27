using System.Text.Json;
using System.Text.Json.Serialization;

namespace BaseFaq.Common.EntityFramework.Core.AutoHistory;

public sealed class AutoHistoryOption
{
    /// <summary>
    ///     Prevent constructor from being called eternally.
    /// </summary>
    private AutoHistoryOption()
    {
    }

    /// <summary>
    ///     The json setting for the 'Changed' column
    /// </summary>
    public JsonSerializerOptions JsonSerializerOptions { get; } = new JsonSerializerOptions
    {
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
        WriteIndented = true
    };

    /// <summary>
    ///     The shared instance of the AutoHistoryOptions.
    /// </summary>
    internal static AutoHistoryOption Instance { get; } = new();

    /// <summary>
    ///     The maximum length of the 'Changed' column. <c>null</c> will use default setting 2048 unless ChangedVarcharMax is
    ///     true
    ///     in which case the column will be varchar(max). Default: null.
    /// </summary>
    public int? ChangedMaxLength { get; set; }

    /// <summary>
    ///     Set this to true to enforce ChangedMaxLength. If this is false, ChangedMaxLength will be ignored.
    ///     Default: true.
    /// </summary>
    public bool LimitChangedLength { get; set; } = true;

    /// <summary>
    ///     The max length for the row id column. Default: 50.
    /// </summary>
    public int RowIdMaxLength { get; set; } = 150;

    /// <summary>
    ///     The max length for the table column. Default: 128.
    /// </summary>
    public int TableMaxLength { get; set; } = 128;

    public string TableName { get; set; } = "__ChangeHistory";
}