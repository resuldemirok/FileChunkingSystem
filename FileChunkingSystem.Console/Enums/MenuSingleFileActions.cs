using System.ComponentModel;

namespace FileChunkingSystem.Console.Enums;

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
