namespace FileChunkingSystem.Application.Models;

public class CleanupOptionsModel : BaseModel
{
    public int? DeleteOlderThanDays { get; set; }
    public long? DeleteLargerThanBytes { get; set; }
    public long? DeleteSmallerThanBytes { get; set; }
    public int? DeleteUnusedForDays { get; set; }
    public List<string>? DeleteWithStatus { get; set; }
    public List<string>? DeleteWithExtensions { get; set; }
    public bool DryRun { get; set; } = true;
    public bool DeleteEmptyChunks { get; set; } = false;
    public bool DeleteCorruptedFiles { get; set; } = false;
    public bool DeleteIncompleteFiles { get; set; } = false;
    public int? MaxFilesToDelete { get; set; }
    public long? MaxBytesToDelete { get; set; }
    public bool ConfirmEachFile { get; set; } = false;
    public bool CreateBackup { get; set; } = false;
    public string? BackupPath { get; set; }
}
