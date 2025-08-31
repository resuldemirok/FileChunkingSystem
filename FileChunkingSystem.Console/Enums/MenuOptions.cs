using System.ComponentModel;

namespace FileChunkingSystem.Console.Enums;

public enum MenuOptions
{
    [Description("Upload and Chunk File")]
    Upload,

    [Description("Merge and Download File")]
    Merge,

    [Description("List All Files")]
    List,

    [Description("Performance Report")]
    Report,

    [Description("Delete File")]
    Delete,

    [Description("Exit")]
    Exit
}
