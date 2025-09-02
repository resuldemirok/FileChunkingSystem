using FileChunkingSystem.Domain.Enums;

namespace FileChunkingSystem.Domain.Entities;

/// <summary>
/// Entity representing metadata for a complete file
/// </summary>
public class FileMetadata : BaseEntity
{
    /// <summary>
    /// Gets or sets the original filename
    /// </summary>
    public string OriginalFileName { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the file extension
    /// </summary>
    public string FileExtension { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the total file size in bytes
    /// </summary>
    public long FileSize { get; set; }
    
    /// <summary>
    /// Gets or sets the SHA256 checksum of the complete file
    /// </summary>
    public string ChecksumSha256 { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the chunking algorithm used for this file
    /// </summary>
    public ChunkingAlgorithm ChunkingAlgorithm { get; set; }
    
    /// <summary>
    /// Gets or sets the total number of chunks
    /// </summary>
    public int TotalChunks { get; set; }
    
    /// <summary>
    /// Gets or sets the size of each chunk in bytes
    /// </summary>
    public int ChunkSize { get; set; }
    
    /// <summary>
    /// Gets or sets the collection of chunk metadata
    /// </summary>
    public virtual List<ChunkMetadata> Chunks { get; set; } = new();
}
