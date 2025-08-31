using System.ComponentModel;

namespace FileChunkingSystem.Console.Enums;

public enum MenuFileUploadActions
{
    [Description("Add file")]
    AddFile,

    [Description("Start process")]
    StartProcess,

    [Description("Cancel")]
    Cancel
}
