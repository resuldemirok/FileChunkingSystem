using System.ComponentModel;

namespace FileChunkingSystem.Console.Enums;

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
