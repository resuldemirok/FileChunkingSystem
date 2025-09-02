namespace FileChunkingSystem.Application.Models;

/// <summary>
/// Model representing parameters for file upload operations
/// </summary>
public class FileUploadModel : BaseModel
{
    /// <summary>
    /// Gets or sets the list of files to upload
    /// </summary>
    public List<FileInputModel> Files { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the chunk size in bytes (optional)
    /// </summary>
    public int? ChunkSize { get; set; }
}
