namespace FileChunkingSystem.Application.Models;

/// <summary>
/// Model representing cleanup options for file maintenance operations
/// </summary>
public class CleanupOptionsModel : BaseModel
{
    /// <summary>
    /// Gets or sets the number of days after which files should be deleted
    /// </summary>
    public int? DeleteOlderThanDays { get; set; }
    
    /// <summary>
    /// Gets or sets the maximum file size in bytes for deletion
    /// </summary>
    public long? DeleteLargerThanBytes { get; set; }
    
    /// <summary>
    /// Gets or sets the minimum file size in bytes for deletion
    /// </summary>
    public long? DeleteSmallerThanBytes { get; set; }
    
    /// <summary>
    /// Gets or sets the number of days a file must be unused before deletion
    /// </summary>
    public int? DeleteUnusedForDays { get; set; }
    
    /// <summary>
    /// Gets or sets the list of file statuses that should be deleted
    /// </summary>
    public List<string>? DeleteWithStatus { get; set; }
    
    /// <summary>
    /// Gets or sets the list of file extensions that should be deleted
    /// </summary>
    public List<string>? DeleteWithExtensions { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether this is a dry run (no actual deletion)
    /// </summary>
    public bool DryRun { get; set; } = true;
    
    /// <summary>
    /// Gets or sets a value indicating whether empty chunks should be deleted
    /// </summary>
    public bool DeleteEmptyChunks { get; set; } = false;
    
    /// <summary>
    /// Gets or sets a value indicating whether corrupted files should be deleted
    /// </summary>
    public bool DeleteCorruptedFiles { get; set; } = false;
    
    /// <summary>
    /// Gets or sets a value indicating whether incomplete files should be deleted
    /// </summary>
    public bool DeleteIncompleteFiles { get; set; } = false;
    
    /// <summary>
    /// Gets or sets the maximum number of files to delete in one operation
    /// </summary>
    public int? MaxFilesToDelete { get; set; }
    
    /// <summary>
    /// Gets or sets the maximum number of bytes to delete in one operation
    /// </summary>
    public long? MaxBytesToDelete { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether to confirm each file deletion
    /// </summary>
    public bool ConfirmEachFile { get; set; } = false;
    
    /// <summary>
    /// Gets or sets a value indicating whether to create a backup before deletion
    /// </summary>
    public bool CreateBackup { get; set; } = false;
    
    /// <summary>
    /// Gets or sets the backup path for deleted files
    /// </summary>
    public string? BackupPath { get; set; }
}
