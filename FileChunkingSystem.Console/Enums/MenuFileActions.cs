using System.ComponentModel;

namespace FileChunkingSystem.Console.Enums;

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
