using System.ComponentModel;

namespace FileChunkingSystem.Console.Enums;

/// <summary>
/// Defines the available export format options for file data.
/// </summary>
public enum MenuExportOptions
{
    [Description("CSV")]
    CSV,

    [Description("JSON")]
    JSON,

    [Description("XML")]
    XML
}
