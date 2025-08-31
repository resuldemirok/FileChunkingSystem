using System.ComponentModel;

namespace FileChunkingSystem.Console.Enums;

public enum MenuExportOptions
{
    [Description("CSV")]
    CSV,

    [Description("JSON")]
    JSON,

    [Description("XML")]
    XML
}
