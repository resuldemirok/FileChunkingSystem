using System.ComponentModel;

namespace FileChunkingSystem.Console.Enums;

/// <summary>
/// Defines the available actions that can be performed on a single file.
/// </summary>
public enum MenuSingleFileActions
{
    [Description("Download/Merge")]
    DownloadMerge,

    [Description("Delete")]
    Delete,

    [Description("Update Metadata")]
    UpdateMetadata,

    [Description("Verify Integrity")]
    VerifyIntegrity
}
