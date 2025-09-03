using System.ComponentModel;

namespace FileChunkingSystem.Console.Enums;

/// <summary>
/// Defines the available actions that can be performed on files.
/// </summary>
public enum MenuFileActions
{
    [Description("View Details")]
    ViewDetails,

    [Description("Download/Merge")]
    DownloadMerge,

    [Description("Delete")]
    Delete,

    [Description("Update Metadata")]
    UpdateMetadata
}
