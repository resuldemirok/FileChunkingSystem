using System.ComponentModel;

namespace FileChunkingSystem.Console.Enums;

/// <summary>
/// Defines the available export format options for performance reports.
/// </summary>
public enum MenuPerformanceExportOptions
{
    [Description("JSON")]
    JSON,

    [Description("CSV")]
    CSV,

    [Description("HTML")]
    HTML,

    [Description("TXT")]
    TXT
}
