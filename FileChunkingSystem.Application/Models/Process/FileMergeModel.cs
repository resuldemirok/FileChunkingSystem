namespace FileChunkingSystem.Application.Models;

/// <summary>
/// Model representing parameters for file merging operations
/// </summary>
public class FileMergeModel : BaseModel
{
    /// <summary>
    /// Gets or sets the identifier of the file metadata to merge
    /// </summary>
    public Guid FileMetadataId { get; set; }
    
    /// <summary>
    /// Gets or sets the output path for the merged file
    /// </summary>
    public string OutputPath { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets a value indicating whether to overwrite existing files
    /// </summary>
    public bool OverwriteIfExists { get; set; } = false;
}
