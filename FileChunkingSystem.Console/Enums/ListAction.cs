using System.ComponentModel;

namespace FileChunkingSystem.Console.Enums;

/// <summary>
/// Defines the available actions for file list operations in the console application.
/// </summary>
public enum ListAction
{
    [Description("View All Files")]
    ViewAll,

    [Description("Search Files")]
    Search,

    [Description("Filter Files by Criteria")]
    Filter,

    [Description("View File Details")]
    Details,

    [Description("Export File List")]
    Export,

    [Description("Cleanup Old Files")]
    Cleanup
}
