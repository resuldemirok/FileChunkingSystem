using System.ComponentModel;

namespace FileChunkingSystem.Console.Enums;

/// <summary>
/// Defines the available file upload action options in the console interface.
/// </summary>
public enum MenuFileUploadActions
{
    [Description("Add file")]
    AddFile,

    [Description("Start process")]
    StartProcess,

    [Description("Cancel")]
    Cancel
}
