namespace FileChunkingSystem.Application.Models;

/// <summary>
/// Model representing input file data for processing
/// </summary>
public class FileInputModel : BaseModel
{
    /// <summary>
    /// Gets or sets the filename
    /// </summary>
    public string FileName { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the file content as byte array
    /// </summary>
    public byte[] Content { get; set; } = Array.Empty<byte>();
}
